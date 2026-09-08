using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    [SerializeField] private string difficultySuffix = "_easy"; // например "_easy", "_normal", "_hard"

    private string baseChartId;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void SetBaseChartId(string id)
    {
        baseChartId = id;
    }

    private void OnClick()
    {
        if (string.IsNullOrEmpty(baseChartId))
        {
            Debug.LogError("DifficultyButton: базовый ID не установлен!");
            return;
        }

        string fullChartId = baseChartId + difficultySuffix; // например "dubstep_easy"
        GameEvents.RiseSelectChart(fullChartId);
    }
}
