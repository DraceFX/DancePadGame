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

    private bool initialized;

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

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        double currentTime = clock.SongTime;
        double spawnTime = note.Time - spawnLeadTime;
        double progress = (currentTime - spawnTime) / spawnLeadTime;

        progress = Mathf.Clamp01((float)progress);

        transform.position = Vector3.Lerp(spawnPosition, targetPosition, (float)progress);
    }

    public void SetDirection(DancePadDirection direction)
    {
        // Здесь позже можно менять Sprite в зависимости от направления.
    }

    public void OnHit(HitResult result)
    {
        initialized = false;
    }

    public void OnMiss()
    {
        initialized = false;
    }

    public void ResetView()
    {
        note = null;
        initialized = false;

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(false);
    }
}
