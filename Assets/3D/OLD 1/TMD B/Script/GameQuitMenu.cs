using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameQuitMenu : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;

    private void Start()
    {
        if (yesButton != null) yesButton.onClick.AddListener(QuitGame);
        if (noButton != null) noButton.onClick.AddListener(ClosePanel);
    }

    void QuitGame()
    {
        Debug.Log("Quit From Exit Panel");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
