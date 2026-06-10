using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif
using UnityEngine.XR;
using UnityEngine.XR.Management;

[DefaultExecutionOrder(-10000)]
public sealed class VRPerformanceManager : MonoBehaviour
{
    // ------ Mode ------
    public enum Role
    {
        Manager,   // One per scene/app. Applies global XR/URP/perf and updates targets.
        Target     // Attach to any object you want optimized. Still one script total.
    }

    [Header("Mode")]
    [Tooltip("Manager: put on a single GameObject in the startup scene. Target: put on any object you want optimized.")]
    public Role role = Role.Manager;

    [Header("Framerate")]
    [Tooltip("Desired target framerate for VR. Common: 72, 80, 90, 120.")]
    [SerializeField] private int targetFrameRate = 90;
    [Tooltip("Disable VSync to let Application.targetFrameRate take effect.")]
    [SerializeField] private bool disableVSync = true;

    [Header("XR Render Scale")]
    [Range(0.5f, 2f)]
    [Tooltip("Render scale for XR eye textures. 1 = default. Lower for performance, higher for quality.")]
    [SerializeField] private float xrEyeTextureScale = 0.95f;

#if UNITY_URP
    [Header("URP Render Scale (if URP is active)")]
    [Range(0.5f, 2f)]
    [Tooltip("URP render scale. Applied when current pipeline is a UniversalRenderPipelineAsset.")]
    [SerializeField] private float urpRenderScale = 0.95f;
#endif

    [Header("Dynamic Resolution")]
    [Tooltip("Enable dynamic resolution using ScalableBufferManager.")]
    [SerializeField] private bool enableDynamicResolution = true;
    [Range(0.5f, 1.0f)]
    [SerializeField] private float minDynamicScale = 0.7f;
    [Range(1.0f, 2.0f)]
    [SerializeField] private float maxDynamicScale = 1.0f;

    [Header("Physics")]
    [Tooltip("Adjust Time.fixedDeltaTime to align with headset refresh. 90Hz ≈ 0.01111, 72Hz ≈ 0.01388. Leave 0 to skip.")]
    [SerializeField] private float fixedDeltaTimeOverride = 0f;

    [Header("Power/Device")]
    [Tooltip("Prevent device from sleeping.")]
    [SerializeField] private bool preventSleep = true;

    [Header("Vendor-specific (Optional via Reflection)")]
    [Tooltip("Enable Fixed Foveated Rendering if supported (Oculus/OpenXR vendors).")]
    [SerializeField] private bool tryEnableFixedFoveatedRendering = true;
    [Tooltip("Foveation level (0-4 typical on Oculus). 0 disables if applied.")]
    [Range(0, 4)]
    [SerializeField] private int ffrLevel = 3;

    [Header("Culling System Settings")]
    [SerializeField] private Camera xrCamera;
    [SerializeField] private int objectsPerFrame = 256;
    [SerializeField] private int boundsRefreshInterval = 90;

    // ------ Target settings (used when role == Target) ------
    [Header("Distances (Target)")]
    [Tooltip("Within this distance, everything is fully enabled.")]
    public float enableDistance = 10f;
    [Tooltip("Beyond this distance, heavy systems disable but renderers may remain on for silhouette.")]
    public float optimizeDistance = 20f;
    [Tooltip("Beyond this distance, object is fully culled (all disabled).")]
    public float cullDistance = 35f;

    [Header("Frustum Rules (Target)")]
    [Tooltip("If object is outside camera frustum, apply optimization tier even if inside optimizeDistance.")]
    public bool optimizeWhenOffscreen = true;
    [Tooltip("If object is outside camera frustum and beyond enableDistance, fully cull it.")]
    public bool cullWhenOffscreen = true;

    [Header("What to toggle (Target)")]
    public bool toggleRenderers = true;
    public bool toggleShadowsFar = true;
    public bool toggleAnimators = true;
    public bool toggleParticleSystems = true;
    public bool toggleLights = true;
    public bool toggleAudioSources = true;
    public bool toggleRigidbodiesSleep = true;

    [Header("Renderer fallback (Target)")]
    [Tooltip("Keep lightweight proxies when Optimized. Leave empty if not needed.")]
    public Renderer[] proxyRenderers = System.Array.Empty<Renderer>();

    [Header("Animator Throttling (Target)")]
    public float fullRateDistance = 8f;
    public float halfRateDistance = 16f;
    public float quarterRateDistance = 24f;
    public float stopDistance = 32f;
    public bool throttleUseUnscaledTime = false;

    [Header("Logging")]
    [SerializeField] private bool enableLogging = true;

    private readonly List<IDisposable> _disposables = new List<IDisposable>();

    // ------ Internal (Manager) ------
    private static VRPerformanceManager s_manager;
    private static readonly List<VRPerformanceManager> s_targets = new List<VRPerformanceManager>(1024);
    private Plane[] _frustumPlanes = new Plane[6];
    private int _cursor;
    private int _frameCount;

    // ------ Internal (Target) ------
    private Renderer[] _renderers = System.Array.Empty<Renderer>();
    private Animator[] _animators = System.Array.Empty<Animator>();
    private ParticleSystem[] _particles = System.Array.Empty<ParticleSystem>();
    private Light[] _lights = System.Array.Empty<Light>();
    private AudioSource[] _audios = System.Array.Empty<AudioSource>();
    private Rigidbody[] _rigidbodies = System.Array.Empty<Rigidbody>();
    private Bounds _worldBounds;
    private float _lastAnimTickTime;

    // ------ Object Pooling (Manager) ------
    [Header("Object Pooling (Manager)")]
    [Tooltip("Default initial size when registering a new prefab pool if not specified.")]
    [SerializeField] private int defaultPoolSize = 16;
    [Tooltip("Allow pool to expand when empty.")]
    [SerializeField] private bool poolAllowExpand = true;
    [Tooltip("Optional parent transform for pooled instances.")]
    [SerializeField] private Transform poolRoot;

    private readonly Dictionary<GameObject, Pool> _prefabToPool = new Dictionary<GameObject, Pool>(64);

    // ------ Task Scheduler (Manager) ------
    [Header("Task Scheduler (Manager)")]
    [Tooltip("How many queued Actions to execute per frame.")]
    [SerializeField] private int actionsPerFrame = 64;
    [Tooltip("How many new coroutines to start per frame from the queue.")]
    [SerializeField] private int coroutinesStartPerFrame = 8;

    private readonly Queue<System.Action> _actionQueue = new Queue<System.Action>(256);
    private readonly Queue<System.Collections.IEnumerator> _coroutineQueue = new Queue<System.Collections.IEnumerator>(128);

    private void Awake()
    {
        if (role == Role.Manager)
        {
            s_manager = this;
            ApplyPerformanceSettings();
            ConfigureCullingSystem();
            if (poolRoot == null)
            {
                var go = new GameObject("PoolRoot");
                go.transform.SetParent(transform, false);
                poolRoot = go.transform;
            }
        }
        else
        {
            CollectComponents();
            RegisterTarget();
        }
    }

    private void OnDestroy()
    {
        if (role == Role.Manager)
        {
            foreach (var d in _disposables)
            {
                d?.Dispose();
            }
            _disposables.Clear();
            if (s_manager == this) s_manager = null;
        }
        else
        {
            UnregisterTarget();
        }
    }

    public void ApplyPerformanceSettings()
    {
        TrySetSleep();
        TrySetVSyncAndFramerate();
        TrySetXREyeTextureScale();
        TrySetDynamicResolution();
#if UNITY_URP
        TrySetURPRenderScale();
#endif
        TrySetFixedDeltaTime();
        TryEnableFoveatedRenderingIfAvailable();
        Log("VRPerformanceManager applied.");
    }

    private void TrySetSleep()
    {
        if (preventSleep)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Log("SleepTimeout set to NeverSleep");
        }
    }

    private void TrySetVSyncAndFramerate()
    {
        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
            Log("VSync disabled (vSyncCount = 0)");
        }

        if (targetFrameRate > 0)
        {
            Application.targetFrameRate = targetFrameRate;
            Log($"Application.targetFrameRate = {targetFrameRate}");
        }
    }

    private void TrySetXREyeTextureScale()
    {
        // XRSettings.eyeTextureResolutionScale works when legacy XR or certain providers are active.
        try
        {
            if (xrEyeTextureScale > 0f && xrEyeTextureScale != 1.0f)
            {
                bool applied = false;
                // Prefer XRDisplaySubsystem when available
                var displaySubsystem = GetActiveDisplaySubsystem();
                if (displaySubsystem != null)
                {
                    // scaleOfAllRenderTargets is typically [0.1, 1.0], clamp accordingly
                    float clamped = Mathf.Clamp(xrEyeTextureScale, 0.5f, 2.0f);
                    // For XRDisplaySubsystem, use SetRenderPassScale if available via reflection (not public in all versions)
                    var prop = displaySubsystem.GetType().GetProperty("scaleOfAllRenderTargets");
                    if (prop != null && prop.CanWrite)
                    {
                        float scaled = Mathf.Clamp(clamped, 0.5f, 1.5f);
                        prop.SetValue(displaySubsystem, scaled, null);
                        applied = true;
                        Log($"XRDisplaySubsystem.scaleOfAllRenderTargets = {scaled:0.00}");
                    }
                }

#pragma warning disable CS0618
                if (!applied && XRSettings.enabled)
                {
                    XRSettings.eyeTextureResolutionScale = xrEyeTextureScale;
                    applied = true;
                    Log($"XRSettings.eyeTextureResolutionScale = {xrEyeTextureScale:0.00}");
                }
#pragma warning restore CS0618

                if (!applied)
                {
                    Log("XR eye texture scale not applied (XR not initialized yet). Will retry on start.");
                    _disposables.Add(CallNextFrame(TrySetXREyeTextureScale));
                }
            }
        }
        catch (Exception e)
        {
            Log($"TrySetXREyeTextureScale exception: {e.Message}");
        }
    }

    private void TrySetDynamicResolution()
    {
        if (!enableDynamicResolution)
            return;

        float min = Mathf.Clamp(minDynamicScale, 0.5f, 1.0f);
        float max = Mathf.Max(min, Mathf.Clamp(maxDynamicScale, 1.0f, 2.0f));
        try
        {
            ScalableBufferManager.ResizeBuffers(min, max);
            Log($"Dynamic Resolution enabled via ScalableBufferManager: {min:0.00}-{max:0.00}");
        }
        catch (Exception e)
        {
            Log($"Dynamic Resolution not supported on this platform: {e.Message}");
        }
    }

#if UNITY_URP
    private void TrySetURPRenderScale()
    {
        var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        var urp = rp as UniversalRenderPipelineAsset;
        if (urp == null)
        {
            Log("URP not active; skipping URP render scale.");
            return;
        }

        float clamped = Mathf.Clamp(urpRenderScale, 0.5f, 2f);
        if (Math.Abs(urp.renderScale - clamped) > 0.0001f)
        {
            urp.renderScale = clamped;
            Log($"URP renderScale = {clamped:0.00}");
        }
    }
#endif

    private void TrySetFixedDeltaTime()
    {
        if (fixedDeltaTimeOverride > 0f)
        {
            Time.fixedDeltaTime = fixedDeltaTimeOverride;
            Log($"Time.fixedDeltaTime = {fixedDeltaTimeOverride:0.00000}s");
        }
        else
        {
            // Reasonable default for VR if user chose specific target framerate
            if (targetFrameRate >= 90)
            {
                Time.fixedDeltaTime = 1f / 90f; // 0.01111
                Log("Time.fixedDeltaTime = 1/90s (auto)");
            }
            else if (targetFrameRate >= 72)
            {
                Time.fixedDeltaTime = 1f / 72f; // 0.01388
                Log("Time.fixedDeltaTime = 1/72s (auto)");
            }
        }
    }

    private void TryEnableFoveatedRenderingIfAvailable()
    {
        if (!tryEnableFixedFoveatedRendering)
            return;

        // Attempt Oculus Integration: OVRManager.fixedFoveatedRenderingLevel
        try
        {
            var ovrManagerType = Type.GetType("OVRManager, Assembly-CSharp", false)
                                  ?? Type.GetType("OVRManager, Oculus.VR", false);
            if (ovrManagerType != null)
            {
                var enumType = ovrManagerType.Assembly.GetType("OVRManager+FixedFoveatedRenderingLevel");
                if (enumType != null)
                {
                    int level = Mathf.Clamp(ffrLevel, 0, 4);
                    object enumValue = Enum.ToObject(enumType, level);
                    var prop = ovrManagerType.GetProperty("fixedFoveatedRenderingLevel");
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(null, enumValue, null);
                        Log($"OVR FFR level set to {level}");
                        return;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log($"OVR FFR reflection failed: {e.Message}");
        }

        // Attempt OpenXR Meta/HTC vendors via XRDisplaySubsystem foveation (not standardized; keep silent if not found)
        try
        {
            var displaySubsystem = GetActiveDisplaySubsystem();
            if (displaySubsystem != null)
            {
                var method = displaySubsystem.GetType().GetMethod("TrySetFoveatedRenderingLevel");
                if (method != null)
                {
                    object[] args = { Mathf.Clamp(ffrLevel, 0, 4) };
                    bool ok = (bool)method.Invoke(displaySubsystem, args);
                    if (ok)
                    {
                        Log($"XRDisplaySubsystem FFR level set to {ffrLevel}");
                        return;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Ignore – optional vendor path
        }
    }

    private XRDisplaySubsystem GetActiveDisplaySubsystem()
    {
        var subsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(subsystems);
        return subsystems.FirstOrDefault(s => s != null && s.running);
    }

    private IDisposable CallNextFrame(Action action)
    {
        return new NextFrameInvoker(this, action);
    }

    private void Log(string msg)
    {
        if (enableLogging)
        {
            Debug.Log($"[VRPerformanceManager] {msg}");
        }
    }

    private void LateUpdate()
    {
        if (role != Role.Manager)
        {
            return;
        }

        if (xrCamera == null)
        {
            xrCamera = Camera.main;
            if (xrCamera == null) return;
        }

        GeometryUtility.CalculateFrustumPlanes(xrCamera, _frustumPlanes);

        int toProcess = Mathf.Min(objectsPerFrame, s_targets.Count);
        _frameCount++;
        for (int i = 0; i < toProcess; i++)
        {
            if (s_targets.Count == 0) break;
            if (_cursor >= s_targets.Count) _cursor = 0;
            var target = s_targets[_cursor++];
            if (!target) continue;
            UpdateOneTarget(target);
        }

        // Run scheduled work with budgets
        RunScheduledActions();
        StartQueuedCoroutines();
    }

    private void UpdateOneTarget(VRPerformanceManager target)
    {
        if (_frameCount % Mathf.Max(1, boundsRefreshInterval) == 0)
        {
            target._worldBounds = target.CalculateWorldBounds();
        }

        Vector3 center = target.transform.position;
        float dist = Vector3.Distance(center, xrCamera.transform.position);
        bool insideFrustum = GeometryUtility.TestPlanesAABB(_frustumPlanes, target._worldBounds);

        CullTier tier;
        if (dist <= target.enableDistance)
        {
            tier = CullTier.Enabled;
            if (!insideFrustum && target.optimizeWhenOffscreen)
            {
                tier = CullTier.Enabled;
            }
        }
        else if (dist <= target.optimizeDistance)
        {
            tier = insideFrustum ? CullTier.Optimized : (target.cullWhenOffscreen ? CullTier.Culled : CullTier.Optimized);
        }
        else if (dist <= target.cullDistance)
        {
            tier = insideFrustum ? CullTier.Optimized : CullTier.Culled;
        }
        else
        {
            tier = CullTier.Culled;
        }

        target.ApplyState(tier, dist);
    }

    // ---------- Target helpers ----------
    private void CollectComponents()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _animators = GetComponentsInChildren<Animator>(true);
        _particles = GetComponentsInChildren<ParticleSystem>(true);
        _lights = GetComponentsInChildren<Light>(true);
        _audios = GetComponentsInChildren<AudioSource>(true);
        _rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        _worldBounds = CalculateWorldBounds();
    }

    private Bounds CalculateWorldBounds()
    {
        bool hasBounds = false;
        var b = new Bounds(transform.position, Vector3.zero);
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (!r) continue;
            if (!hasBounds)
            {
                b = r.bounds;
                hasBounds = true;
            }
            else
            {
                b.Encapsulate(r.bounds);
            }
        }
        if (!hasBounds) b = new Bounds(transform.position, Vector3.one);
        return b;
    }

    private enum CullTier { Enabled, Optimized, Culled }

    private void ApplyState(CullTier tier, float distance)
    {
        bool enableFull = tier == CullTier.Enabled;
        bool optimize = tier == CullTier.Optimized;
        bool culled = tier == CullTier.Culled;

        if (toggleRenderers)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (!r) continue;
                r.enabled = !culled && (enableFull || optimize);
                if (toggleShadowsFar)
                {
                    r.shadowCastingMode = (optimize || culled)
                        ? UnityEngine.Rendering.ShadowCastingMode.Off
                        : UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }
            if (proxyRenderers != null && proxyRenderers.Length > 0)
            {
                for (int i = 0; i < proxyRenderers.Length; i++)
                {
                    var pr = proxyRenderers[i];
                    if (!pr) continue;
                    pr.enabled = optimize && !culled;
                }
            }
        }

        if (toggleAnimators)
        {
            float rate = GetThrottleRate(distance);
            float baseFrame = 1f / 90f;
            float desiredDelta = Mathf.Max(baseFrame / Mathf.Max(0.0001f, rate), baseFrame);
            float now = throttleUseUnscaledTime ? Time.realtimeSinceStartup : Time.time;
            bool tick = (now - _lastAnimTickTime) >= desiredDelta;

            for (int i = 0; i < _animators.Length; i++)
            {
                var a = _animators[i];
                if (!a) continue;
                if (culled)
                {
                    a.enabled = false;
                    a.cullingMode = AnimatorCullingMode.CullCompletely;
                }
                else
                {
                    a.enabled = enableFull || optimize;
                    a.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    a.speed = tick ? 1f : 0f;
                }
            }
            if (tick) _lastAnimTickTime = now;
        }

        if (toggleParticleSystems)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                var p = _particles[i];
                if (!p) continue;
                var emission = p.emission;
                emission.enabled = !culled && enableFull;
                if (culled)
                {
                    p.Clear(true);
                    p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        if (toggleLights)
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                var l = _lights[i];
                if (!l) continue;
                l.enabled = !culled && (enableFull || optimize);
                if (optimize) l.shadows = LightShadows.None;
            }
        }

        if (toggleAudioSources)
        {
            for (int i = 0; i < _audios.Length; i++)
            {
                var a = _audios[i];
                if (!a) continue;
                if (culled)
                {
                    if (a.isPlaying) a.Pause();
                }
                a.enabled = !culled && (enableFull || optimize);
            }
        }

        if (toggleRigidbodiesSleep)
        {
            for (int i = 0; i < _rigidbodies.Length; i++)
            {
                var rb = _rigidbodies[i];
                if (!rb) continue;
                if (culled || optimize)
                {
                    if (!rb.IsSleeping()) rb.Sleep();
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }
            }
        }
    }

    private float GetThrottleRate(float dist)
    {
        if (dist <= fullRateDistance) return 1f;
        if (dist <= halfRateDistance) return 0.5f;
        if (dist <= quarterRateDistance) return 0.25f;
        if (dist <= stopDistance) return 0.1f;
        return 0f;
    }

    private void RegisterTarget()
    {
        if (!s_targets.Contains(this)) s_targets.Add(this);
    }

    private void UnregisterTarget()
    {
        s_targets.Remove(this);
    }

    private void ConfigureCullingSystem()
    {
        if (xrCamera == null) xrCamera = Camera.main;
        Log("Culling system configured (single-script mode).");
    }

    // ---------- Object Pooling (Manager) ----------
    private sealed class Pool
    {
        public readonly GameObject prefab;
        public readonly Transform parent;
        public readonly bool allowExpand;
        private readonly Queue<GameObject> _free = new Queue<GameObject>();
        private readonly List<GameObject> _all = new List<GameObject>();

        public Pool(GameObject prefab, int initialSize, bool allowExpand, Transform parent, VRPerformanceManager owner)
        {
            this.prefab = prefab;
            this.allowExpand = allowExpand;
            this.parent = parent;
            Prewarm(initialSize, owner);
        }

        public void Prewarm(int count, VRPerformanceManager owner)
        {
            for (int i = 0; i < count; i++)
            {
                var go = owner.InstantiatePooled(prefab, parent);
                go.SetActive(false);
                _free.Enqueue(go);
                _all.Add(go);
            }
        }

        public GameObject Get(VRPerformanceManager owner)
        {
            if (_free.Count > 0)
            {
                var go = _free.Dequeue();
                return go;
            }
            if (allowExpand)
            {
                var go = owner.InstantiatePooled(prefab, parent);
                _all.Add(go);
                return go;
            }
            return null;
        }

        public void Return(GameObject instance)
        {
            if (!instance) return;
            instance.SetActive(false);
            instance.transform.SetParent(parent, false);
            _free.Enqueue(instance);
        }
    }

    private GameObject InstantiatePooled(GameObject prefab, Transform parent)
    {
        var go = Instantiate(prefab, parent);
        var token = go.GetComponent<PoolToken>();
        if (token == null) token = go.AddComponent<PoolToken>();
        token.sourcePrefab = prefab;
        return go;
    }

    private Pool GetOrCreatePool(GameObject prefab, int? initialSize = null, bool? allowExpand = null, Transform parent = null)
    {
        if (prefab == null) return null;
        if (_prefabToPool.TryGetValue(prefab, out var pool)) return pool;

        int size = Mathf.Max(0, initialSize ?? defaultPoolSize);
        bool expand = allowExpand ?? poolAllowExpand;
        var p = new Pool(prefab, size, expand, parent ?? poolRoot, this);
        _prefabToPool[prefab] = p;
        return p;
    }

    // Public Pooling API (call from your gameplay code)
    public static void PoolRegister(GameObject prefab, int initialSize, bool allowExpand = true, Transform parent = null)
    {
        if (s_manager == null) return;
        s_manager.GetOrCreatePool(prefab, initialSize, allowExpand, parent);
    }

    public static GameObject PoolSpawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (s_manager == null || prefab == null) return null;
        var pool = s_manager.GetOrCreatePool(prefab, null, null, parent);
        var go = pool.Get(s_manager);
        if (go == null) return null;
        go.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) go.transform.SetParent(parent, true);
        go.SetActive(true);
        return go;
    }

    public static void PoolDespawn(GameObject instance)
    {
        if (s_manager == null || instance == null) return;
        var token = instance.GetComponent<PoolToken>();
        if (token == null || token.sourcePrefab == null) { instance.SetActive(false); return; }
        if (!s_manager._prefabToPool.TryGetValue(token.sourcePrefab, out var pool))
        {
            // Create pool entry if missing, then return
            pool = s_manager.GetOrCreatePool(token.sourcePrefab, 0, true, s_manager.poolRoot);
        }
        pool.Return(instance);
    }

    private sealed class PoolToken : MonoBehaviour
    {
        public GameObject sourcePrefab;
    }

    // ---------- Task Scheduler (Manager) ----------
    public static void EnqueueAction(System.Action action)
    {
        if (s_manager == null || action == null) return;
        s_manager._actionQueue.Enqueue(action);
    }

    public static void EnqueueCoroutine(System.Collections.IEnumerator routine)
    {
        if (s_manager == null || routine == null) return;
        s_manager._coroutineQueue.Enqueue(routine);
    }

    public static Coroutine StartThrottledForEach<T>(IEnumerable<T> items, int itemsPerFrame, System.Action<T> body)
    {
        if (s_manager == null) return null;
        return s_manager.StartCoroutine(s_manager.ForEachRoutine(items, Mathf.Max(1, itemsPerFrame), body));
    }

    public static Coroutine RunEverySeconds(System.Action tick, float seconds)
    {
        if (s_manager == null) return null;
        return s_manager.StartCoroutine(s_manager.RunEverySecondsRoutine(tick, Mathf.Max(0.01f, seconds)));
    }

    private void RunScheduledActions()
    {
        int budget = Mathf.Max(0, actionsPerFrame);
        while (budget-- > 0 && _actionQueue.Count > 0)
        {
            var a = _actionQueue.Dequeue();
            try { a?.Invoke(); } catch (System.Exception e) { Log($"Action error: {e.Message}"); }
        }
    }

    private void StartQueuedCoroutines()
    {
        int budget = Mathf.Max(0, coroutinesStartPerFrame);
        while (budget-- > 0 && _coroutineQueue.Count > 0)
        {
            var r = _coroutineQueue.Dequeue();
            StartCoroutine(r);
        }
    }

    private System.Collections.IEnumerator ForEachRoutine<T>(IEnumerable<T> items, int itemsPerFrame, System.Action<T> body)
    {
        if (items == null || body == null) yield break;
        int count = 0;
        foreach (var it in items)
        {
            body(it);
            count++;
            if (count >= itemsPerFrame)
            {
                count = 0;
                yield return null;
            }
        }
    }

    private System.Collections.IEnumerator RunEverySecondsRoutine(System.Action tick, float seconds)
    {
        var wait = new WaitForSeconds(seconds);
        while (true)
        {
            try { tick?.Invoke(); } catch (System.Exception e) { Log($"Tick error: {e.Message}"); }
            yield return wait;
        }
    }

    private sealed class NextFrameInvoker : MonoBehaviour, IDisposable
    {
        private Action _action;

        public NextFrameInvoker(MonoBehaviour owner, Action action)
        {
            _action = action;
            if (owner != null && owner.gameObject != null)
            {
                owner.StartCoroutine(InvokeCo());
            }
        }

        private System.Collections.IEnumerator InvokeCo()
        {
            yield return null;
            try { _action?.Invoke(); }
            finally { _action = null; }
        }

        public void Dispose()
        {
            _action = null;
        }
    }
}


