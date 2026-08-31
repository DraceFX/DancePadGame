using System.Collections;
using UnityEngine;

public class RhythmGameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private RhythmClock rhythmClock;
    [SerializeField] private AudioSource audioSource;

    [Header("Song")]
    [SerializeField] private string songId = "decadence";

    private RhythmChartData chart;

    private bool musicFinished;

    private void Update()
    {
        CheckMusicFinished();
    }

    private void OnEnable()
    {
        GameEvents.OnStartPlay += StartGame;
        GameEvents.OnSelectChart += (o) => songId = o;
    }

    private void OnDisable()
    {
        GameEvents.OnStartPlay -= StartGame;
        GameEvents.OnSelectChart -= (o) => songId = o;
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        audioSource.Stop();
        musicFinished = false;
        StartCoroutine(LoadSong());
    }

    private IEnumerator LoadSong()
    {
        chart = RhythmSongLoader.LoadChart(songId);
        if (chart == null)
        {
            Debug.LogError($"Failed to load song: {songId}");
            yield break;
        }

        Debug.Log($"Loaded song: {chart.Title}");
        Debug.Log($"Artist: {chart.Artist}");
        Debug.Log($"BPM: {chart.BPM}");
        Debug.Log($"Notes: {chart.Notes.Count}");

        string audioPath = RhythmSongLoader.FindAudioFile(songId);

        if (string.IsNullOrEmpty(audioPath))
        {
            Debug.LogError($"Audio file not found for: {songId}");
            yield break;
        }

        Debug.Log($"Loading audio: {audioPath}");

        yield return StartCoroutine(RhythmAudioLoader.Load(audioPath, OnAudioLoaded));
    }

    private void OnAudioLoaded(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError($"Failed to load audio: {songId}");
            return;
        }

        audioSource.clip = clip;
        musicFinished = false;
        rhythmManager.Initialize(chart);
        StartSong();
    }

    private void StartSong()
    {
        double startDspTime = AudioSettings.dspTime + 0.1;
        audioSource.PlayScheduled(startDspTime);
        rhythmClock.StartAt(startDspTime);
    }

    private void CheckMusicFinished()
    {
        if (musicFinished) return;
        if (!rhythmClock.IsRunning) return;

        // Ждём, пока клип реально начнёт воспроизводиться
        if (audioSource.time <= 0f) return;

        // Если музыка ещё играет — выходим
        if (audioSource.isPlaying) return;

        // Музыка завершилась
        musicFinished = true;
        audioSource.Stop(); // гарантируем остановку
        rhythmClock.Stop();
        GameEvents.RaiseMusicFinished();
        Debug.Log("Finish playing music!");
    }
}
