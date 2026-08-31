using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [Header("Text Reference")]
    [SerializeField] private TMP_Text countdownText;

    [Header("Animation Settings")]
    [SerializeField] private float startScale = 2.0f;
    [SerializeField] private float endScale = 1.0f;
    [SerializeField] private float scaleDuration = 0.4f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float displayDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Sequence sequence;

    [ContextMenu("Start Game")]
    public void StartCountdown()
    {
        StartCoroutine(StartTimer());
    }

    private IEnumerator StartTimer()
    {
        ShowNumber("3");
        yield return new WaitForSeconds(1.5f);

        ShowNumber("2");
        yield return new WaitForSeconds(1.5f);

        ShowNumber("1");
        yield return new WaitForSeconds(1.5f);

        ShowNumber("GO!");
        yield return new WaitForSeconds(1.0f);

        GameEvents.RaiseStartPlay();
    }

    private void ShowNumber(string text)
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }

        countdownText.text = text;
        countdownText.gameObject.SetActive(true);
        countdownText.transform.localScale = Vector3.one * startScale;

        Color startColor = countdownText.color;
        startColor.a = 1f;
        countdownText.color = startColor;

        sequence = DOTween.Sequence();
        sequence.Append(countdownText.transform.DOScale(endScale, scaleDuration).SetEase(scaleCurve));
        sequence.AppendInterval(displayDuration);

        if (fadeOutDuration > 0f)
        {
            Color targetColor = countdownText.color;
            targetColor.a = 0f;
            sequence.Append(DOVirtual.Color(countdownText.color, targetColor, fadeOutDuration, value =>
            {
                countdownText.color = value;
            }));
        }
        else
        {
            sequence.AppendCallback(() => countdownText.gameObject.SetActive(false));
        }

        sequence.OnComplete(() =>
        {
            countdownText.gameObject.SetActive(false);
            if (sequence != null && !sequence.IsActive())
            {
                sequence = null;
            }
        });
    }
}
