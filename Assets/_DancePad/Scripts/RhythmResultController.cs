using TMPro;
using UnityEngine;

public class RhythmResultController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AccuracyManager accuracyManager;
    [SerializeField] private TMP_Text[] results;

    [Header("Result")]
    [SerializeField, Range(0f, 100f)] private float winAccuracy = 65f;

    [Header("GameObjects")]
    [SerializeField] private GameObject winObject;
    [SerializeField] private GameObject loseObject;

    private bool resultShow;

    private void OnEnable()
    {
        GameEvents.OnMusicFinished += OnMusicFinished;
        GameEvents.OnStartPlay += ResetResult;
    }

    private void OnDisable()
    {
        GameEvents.OnMusicFinished -= OnMusicFinished;
        GameEvents.OnStartPlay -= ResetResult;
    }

    private void ResetResult()
    {
        resultShow = false;
        if (winObject != null) winObject.SetActive(false);
        if (loseObject != null) loseObject.SetActive(false);
    }


    private void OnMusicFinished()
    {
        if (resultShow) return;

        resultShow = true;
        float accuracy = accuracyManager.Accuracy;
        foreach (var text in results)
        {
            text.text = $"{accuracy}%";
        }

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
        GameEvents.RaiseWiinGame();
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
        GameEvents.RaiseLoseGame();
    }
}
