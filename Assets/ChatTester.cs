using UnityEngine;

public class ChatTester : MonoBehaviour
{
    [SerializeField] private ChatDisplay chatDisplay; // Hubungkan dari Inspector

    void Start()
    {
        // Simulasi percakapan
        chatDisplay.AddMessage("🧑 User", "Halo Gemini!");
        chatDisplay.AddMessage("🤖 Gemini", "Hai juga! Ada yang bisa saya bantu?");
        chatDisplay.AddMessage("🧑 User", "Tolong jelaskan reaksi asam-basa.");
    }
}
