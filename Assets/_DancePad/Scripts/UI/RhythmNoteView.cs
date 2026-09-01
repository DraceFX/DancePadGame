using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RhythmNoteView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image image;
    [SerializeField] private Image backgroundImage;

    [Header("Hit Effect Settings")]
    [SerializeField] private float hitPunchScale = 1.2f;
    [SerializeField] private float hitDuration = 0.2f;
    [SerializeField] private Color hitColor = Color.green;

    [Header("Miss Effect Settings")]
    [SerializeField] private float missShakeStrength = 10f;
    [SerializeField] private float missDuration = 0.3f;
    [SerializeField] private Color missColor = Color.red;

    private RuntimeRhythmNote note;
    private RhythmClock clock;

    private RectTransform spawnPoint;
    private RectTransform targetPoint;

    private double spawnLeadTime;

    private Vector3 spawnPosition;
    private Vector3 targetPosition;

    private Tweener moveTween;
    private Sequence effectSequence;

    public event Action<RhythmNoteView> AnimationCompleted;

    public void Initialize(RuntimeRhythmNote note, RhythmClock clock, RectTransform spawnPoint, RectTransform targetPoint, double spawnLeadTime)
    {
        this.note = note;
        this.clock = clock;
        this.spawnPoint = spawnPoint;
        this.targetPoint = targetPoint;
        this.spawnLeadTime = spawnLeadTime;


        spawnPosition = spawnPoint.position;
        targetPosition = targetPoint.position;

        transform.position = spawnPosition;

        double currentTime = clock.SongTime;
        double spawnTime = note.Time - spawnLeadTime;
        float delay = Mathf.Max(0f, (float)(spawnTime - currentTime));

        moveTween = transform.DOMove(targetPosition, (float)spawnLeadTime).SetDelay(delay).SetEase(Ease.Linear);
    }

    public void SetDirection(DancePadDirection direction)
    {
        if (image == null)
        {
            Debug.LogWarning("Image не назначен в RhythmNoteView");
            return;
        }

        float angle = direction switch
        {
            DancePadDirection.Up => 90f,
            DancePadDirection.UpRight => 45f,
            DancePadDirection.Right => 0f,
            DancePadDirection.DownRight => -45f,
            DancePadDirection.Down => -90f,
            DancePadDirection.DownLeft => -135f,
            DancePadDirection.Left => 180f,
            DancePadDirection.UpLeft => 135f,
            _ => 0f
        };
        image.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void OnHit(HitResult result)
    {
        // KillTween();
        KillMoveTween();
        KillEffectTween();

        float punch = hitPunchScale;
        float duration = hitDuration;
        Color color = hitColor;

        if (result == HitResult.Perfect)
        {
            punch = 1.4f;
            duration = 0.25f;
            color = Color.cyan;
        }
        else if (result == HitResult.Good)
        {
            punch = 1.2f;
            duration = 0.2f;
            color = Color.green;
        }

        effectSequence = DOTween.Sequence();
        effectSequence.Append(transform.DOPunchScale(Vector3.one * punch, duration, 10, 1));
        effectSequence.Join(image.DOColor(color, duration * 0.5f));
        effectSequence.Join(image.DOFade(0f, duration * 0.5f));

        if (backgroundImage != null)
        {
            effectSequence.Join(backgroundImage.DOColor(color, duration * 0.5f));
            effectSequence.Join(backgroundImage.DOFade(0f, duration * 0.5f));
        }

        effectSequence.OnComplete(() =>
        {
            ResetView();
            AnimationCompleted?.Invoke(this);
        });
    }

    public void OnMiss()
    {
        // KillTween();
        KillMoveTween();
        KillEffectTween();

        effectSequence = DOTween.Sequence();
        effectSequence.Append(transform.DOShakePosition(missDuration, missShakeStrength, 20, 90, false, true));
        effectSequence.Join(image.DOColor(missColor, missDuration * 0.5f));
        effectSequence.Join(image.DOFade(0f, missDuration * 0.5f));

        if (backgroundImage != null)
        {
            effectSequence.Join(backgroundImage.DOColor(missColor, missDuration * 0.5f));
            effectSequence.Join(backgroundImage.DOFade(0f, missDuration * 0.5f));
        }

        effectSequence.OnComplete(() =>
        {
            ResetView();
            AnimationCompleted?.Invoke(this);
        });
    }

    public void ResetView()
    {
        note = null;
        // KillTween();
        KillMoveTween();
        KillEffectTween();

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (image != null)
        {
            image.rectTransform.localRotation = Quaternion.identity;
            image.color = Color.white;
        }
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }

        gameObject.SetActive(false);
    }

    private void KillTween()
    {
        if (moveTween != null)
        {
            moveTween.Kill();
            moveTween = null;
        }

        DOTween.Kill(transform);
    }

    private void KillMoveTween()
    {
        if (moveTween != null)
        {
            moveTween.Kill();
            moveTween = null;
        }
    }

    private void KillEffectTween()
    {
        if (effectSequence != null)
        {
            effectSequence.Kill();
            effectSequence = null;
        }
    }
}
