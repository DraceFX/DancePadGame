using TMPro;
using UnityEngine;

public class AccuracyView : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void OnEnable()
    {
        GameEvents.OnAccuracyChanged += OnAccuracyChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnAccuracyChanged -= OnAccuracyChanged;
    }

    private void OnAccuracyChanged(float accuracy)
    {
        text.text = $"{accuracy:F2}%";
    }
}
