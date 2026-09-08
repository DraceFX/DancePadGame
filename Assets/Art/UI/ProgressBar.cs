using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private RhythmClock rhythmClock; 
    [SerializeField] private AudioSource audioSource; 

    private void OnEnable()
    {
        GameEvents.OnStartPlay += HandleStartPlay;
        GameEvents.OnMusicFinished += HandleMusicFinished;
    }

    private void OnDisable()
    {
        GameEvents.OnStartPlay -= HandleStartPlay;
        GameEvents.OnMusicFinished -= HandleMusicFinished;
    }

    private void Update()
    {
        if (progressSlider == null || audioSource == null || rhythmClock == null) return;
        if (!rhythmClock.IsRunning) return;
        if (audioSource.clip == null) return;

        float currentTime = (float)rhythmClock.SongTime;
        float totalTime = audioSource.clip.length;

        if (totalTime <= 0f) return;

        float progress = Mathf.Clamp01(currentTime / totalTime);
        progressSlider.value = progress;
    }

    private void HandleStartPlay()
    {
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
    }

    private void HandleMusicFinished()
    {
        if (progressSlider != null && audioSource != null && audioSource.clip != null)
        {
            progressSlider.value = 1f;
        }
    }
}
