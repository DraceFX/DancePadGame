using UnityEngine;

public class RhythmResultController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AccuracyManager accuracyManager;

    [Header("Result")]
    [SerializeField, Range(0f, 100f)] private float winAccuracy = 65f;

    [Header("GameObjects")]
    [SerializeField] private GameObject winObject;
    [SerializeField] private GameObject loseObject;

    private bool resultShow;

    private void OnEnable()
    {
        GameEvents.OnMusicFinished += OnMusicFinished;
    }

    private void OnDisable()
    {
        GameEvents.OnMusicFinished -= OnMusicFinished;
    }

    private void OnMusicFinished()
    {
        if (resultShow) return;

        resultShow = true;
        float accuracy = accuracyManager.Accuracy;
        if (accuracy >= winAccuracy)
        {
            ShowWin();
        }
        else
        {
            ShowLose();
        }
    }

    private void ShowWin()
    {
        if (winObject != null)
        {
            winObject.SetActive(true);
        }

        if (loseObject != null)
        {
            loseObject.SetActive(false);
        }

        Debug.Log("WIN!");
    }

    private void ShowLose()
    {
        if (winObject != null)
        {
            winObject.SetActive(false);
        }

        if (loseObject != null)
        {
            loseObject.SetActive(true);
        }

        Debug.Log("LOSE!");
    }
}
