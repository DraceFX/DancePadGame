using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RhythmNoteView : MonoBehaviour
{
    [SerializeField] private Image image;

    private RuntimeRhythmNote note;
    private RhythmClock clock;

    private RectTransform spawnPoint;
    private RectTransform targetPoint;

    private double spawnLeadTime;

    private Vector3 spawnPosition;
    private Vector3 targetPosition;

    private Tweener moveTween;

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

        moveTween = transform.DOMove(targetPosition, (float)spawnLeadTime)
            .SetDelay(delay)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // Если нота дошла до цели, но не была обработана — это можно считать промахом (здесь просто ничего не делаем)
            });
    }

    public void SetDirection(DancePadDirection direction)
    {
        // Здесь позже можно менять Sprite в зависимости от направления.
    }

    public void OnHit(HitResult result)
    {
        KillTween();
    }

    public void OnMiss()
    {
        KillTween();
    }

    public void ResetView()
    {
        note = null;
        KillTween();

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(false);
    }

    private void KillTween()
    {
        if (moveTween != null)
        {
            moveTween.Kill();
            moveTween = null;
        }
    }
}
