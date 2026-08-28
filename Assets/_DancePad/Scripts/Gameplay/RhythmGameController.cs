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

    private bool musicFinshed;

    private void Update()
    {
        CheckMusicFinished();
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
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
        if (musicFinshed) return;
        if (!rhythmClock.IsRunning) return;
        if (audioSource.isPlaying) return;

        musicFinshed = true;
        rhythmClock.Stop();
        GameEvents.RaiseMusicFinished();
        Debug.Log("Finish playing music!");
    }
}
