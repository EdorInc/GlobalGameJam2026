using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    private float time = 0f;

    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private float MaxTime = 300;

    private void Start()
    {
        time = MaxTime;
        StartTime();
    }

    public void StartTime()
    {
        InvokeRepeating(nameof(TickTime), 1f, 1f);
    }

    public void IncreaseTimer(float amount)
    {
        time += amount;
        UpdateText();
    }

    private void TickTime()
    {
        time--;
        UpdateText();
    }

    private void UpdateText()
    {
        timeText.text = $"{time}";
    }

    public float GetTime()
    {
        return time;
    }
}
