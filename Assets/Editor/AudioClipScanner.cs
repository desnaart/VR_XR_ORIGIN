using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AudioClipScanner
{
    [MenuItem("Tools/Scan All AudioClip Usage")]
    static void ScanAudio()
    {
        AudioSource[] sources = GameObject.FindObjectsOfType<AudioSource>(true);

        Dictionary<AudioClip, List<GameObject>> map = new Dictionary<AudioClip, List<GameObject>>();

        foreach (var src in sources)
        {
            if (src.clip == null) continue;

            if (!map.ContainsKey(src.clip))
                map[src.clip] = new List<GameObject>();

            map[src.clip].Add(src.gameObject);
        }

        foreach (var pair in map)
        {
            Debug.Log("=== AUDIO CLIP: " + pair.Key.name + " ===");

            foreach (var obj in pair.Value)
            {
                Debug.Log("Used by: " + obj.name, obj);
            }
        }

        Debug.Log("Scan selesai. Total clip: " + map.Count);
    }
}