using UnityEngine;
using UnityEditor;

public class AudioFinderEditor
{
    [MenuItem("Tools/Find PlayOnAwake Audio")]
    static void FindAudio()
    {
        AudioSource[] sources = GameObject.FindObjectsOfType<AudioSource>(true);

        int count = 0;

        foreach (var src in sources)
        {
            if (src.playOnAwake)
            {
                Debug.Log("PlayOnAwake: " + src.gameObject.name, src.gameObject);
                count++;
            }
        }

        Debug.Log("Total PlayOnAwake Audio: " + count);
    }
}