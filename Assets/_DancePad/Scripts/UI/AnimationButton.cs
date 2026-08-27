using System.Collections;
using UnityEngine;

public class AnimationButton : MonoBehaviour
{
    [SerializeField] private DancePadDirection direction;

    [SerializeField] private float punchScale = 0.8f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Coroutine animationCoroutine;

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
    }

    private void HandleDancePadPressed(DancePadDirection pressedDirection)
    {
        if (pressedDirection == direction)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateHit());
        }
    }


    private IEnumerator AnimateHit()
    {
        rectTransform.localScale = originalScale;
        Vector3 targetScale = originalScale * punchScale;

        // Сжатие
        yield return AnimateScale(rectTransform, originalScale, targetScale, duration, animationCurve);
        // Возврат
        yield return AnimateScale(rectTransform, targetScale, originalScale, duration, animationCurve);

        rectTransform.localScale = originalScale;
    }

    private IEnumerator AnimateScale(RectTransform rect, Vector3 from, Vector3 to, float time, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            float curveValue = curve.Evaluate(t);
            rect.localScale = Vector3.LerpUnclamped(from, to, curveValue);
            yield return null;
        }
        rect.localScale = to;
    }
}
