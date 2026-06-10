using UnityEngine;
using TMPro;

public class ExperimentTimer : MonoBehaviour
{
    public float duration = 1800f; // 30 menit
    public TextMeshProUGUI timerText;
    public GameObject timeUpUI;

    float remainingTime;
    bool timerRunning = false;

    public void StartTimer()
    {
        remainingTime = duration;
        timerRunning = true;

        if (timeUpUI != null)
            timeUpUI.SetActive(false);
    }

    void Update()
    {
        if (!timerRunning) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            timerRunning = false;
            TimeUp();
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TimeUp()
    {
        Debug.Log("Waktu habis");

        if (timeUpUI != null)
            timeUpUI.SetActive(true);
    }
}