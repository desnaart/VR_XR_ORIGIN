using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect;      // Drag Scroll View di sini
    [SerializeField] private RectTransform content;      // Drag Content (anak dari Viewport)
    [SerializeField] private GameObject bubbleUserPrefab;   // Drag prefab bubble user
    [SerializeField] private GameObject bubbleGeminiPrefab; // Drag prefab bubble AI

    [Header("Layout Settings")]
    [SerializeField] private bool autoScroll = true;
    [SerializeField] private float maxWidth = 600f; // batas lebar bubble biar tidak melebar ke luar

    public void AddMessage(string sender, string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        // Tentukan prefab
        GameObject prefab = (sender.Contains("Gemini") || sender.Contains("🤖"))
            ? bubbleGeminiPrefab
            : bubbleUserPrefab;

        // Instantiate bubble baru
        GameObject bubbleObj = Instantiate(prefab, content);
        bubbleObj.transform.localScale = Vector3.one;

        // Ambil komponen TMP di dalam bubble
        TMP_Text textComp = bubbleObj.GetComponentInChildren<TMP_Text>(true);
        if (textComp != null)
        {
            textComp.text = message;
            textComp.enableWordWrapping = true;
            textComp.richText = false;
            textComp.ForceMeshUpdate();
        }

        // Atur batas lebar bubble biar tidak keluar dari layar
        LayoutElement layout = bubbleObj.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.minWidth = 100f;
            layout.preferredWidth = Mathf.Min(maxWidth, textComp.preferredWidth + 40f);
        }

        // Update layout dan scroll
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (autoScroll) ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
