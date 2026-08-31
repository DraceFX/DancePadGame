using UnityEngine;
using UnityEngine.UI;

public class SelectChart : MonoBehaviour
{
    [SerializeField] private string chartId = "";

    private Button selectChartButton;

    private void Awake()
    {
        selectChartButton = GetComponent<Button>();
        selectChartButton.onClick.AddListener(() => GameEvents.RiseSelectChart(chartId));
    }
}
