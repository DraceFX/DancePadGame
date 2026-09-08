using UnityEngine;
using UnityEngine.UI;

public class SelectChart : MonoBehaviour
{
    [SerializeField] private string chartId = "";
    [SerializeField] private DifficultyButton[] difficultyButtons;

    private Button selectChartButton;

    private void Awake()
    {
        selectChartButton = GetComponent<Button>();
        selectChartButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        foreach (var button in difficultyButtons)
        {
            button.SetBaseChartId(chartId);
        }
    }
}
