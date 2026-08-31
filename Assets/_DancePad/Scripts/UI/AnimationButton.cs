using System.Collections;
using DG.Tweening;
using UnityEngine;

public class AnimationButton : MonoBehaviour
{
    [SerializeField] private DancePadDirection direction;

    [SerializeField] private float punchScale = 0.8f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Sequence sequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        GameEvents.OnDancePadPressAccepted += HandleDancePadPressed;
    }

    private void OnDisable()
    {
        GameEvents.OnDancePadPressAccepted -= HandleDancePadPressed;
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }
    }

    private void HandleDancePadPressed(DancePadDirection pressedDirection)
    {
        if (pressedDirection == direction)
        {
            if (sequence != null)
            {
                sequence.Kill();
                sequence = null;
            }

            rectTransform.localScale = originalScale;
            Vector3 targetScale = originalScale * punchScale;

            sequence = DOTween.Sequence();

            sequence.Append(rectTransform.DOScale(targetScale, duration).SetEase(animationCurve));
            sequence.Append(rectTransform.DOScale(originalScale, duration).SetEase(animationCurve));

            sequence.OnComplete(() =>
            {
                if (sequence != null && !sequence.IsActive())
                {
                    sequence = null;
                }
            });
        }
    }
}
