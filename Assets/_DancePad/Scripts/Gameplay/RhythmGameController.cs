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
    private Coroutine _finishWatcher;
    private System.Action<string> _onSelectChart;

    private void OnEnable()
    {
        _onSelectChart = (o) => songId = o;
        GameEvents.OnStartPlay += StartGame;
        GameEvents.OnSelectChart += _onSelectChart;
    }

    private void OnDisable()
    {
        GameEvents.OnStartPlay -= StartGame;
        GameEvents.OnSelectChart -= _onSelectChart;

        if (_finishWatcher != null)
        {
            StopCoroutine(_finishWatcher);
            _finishWatcher = null;
        }
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        audioSource.Stop();
        rhythmClock.Stop();
        musicFinished = false;

        if (_finishWatcher != null)
        {
            StopCoroutine(_finishWatcher);
            _finishWatcher = null;
        }

        StartCoroutine(LoadSong());
    }

    private IEnumerator LoadSong()
    {
        chart = RhythmSongLoader.LoadChart(songId);
        if (chart == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to load song: {songId}");
#endif
            yield break;
        }

        string audioPath = RhythmSongLoader.FindAudioFile(songId);
        if (string.IsNullOrEmpty(audioPath))
        {
#if UNITY_EDITOR
            Debug.LogError($"Audio file not found for: {songId}");
#endif
            yield break;
        }

        yield return StartCoroutine(RhythmAudioLoader.Load(audioPath, OnAudioLoaded));
    }

    private void OnAudioLoaded(AudioClip clip)
    {
        if (clip == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to load audio: {songId}");
#endif
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

        if (_finishWatcher != null)
        {
            StopCoroutine(_finishWatcher);
        }
        _finishWatcher = StartCoroutine(WaitForSongEnd(startDspTime));
    }

    private IEnumerator WaitForSongEnd(double startDspTime)
    {
        if (audioSource.clip == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Clip is null in WaitForSongEnd");
#endif
            yield break;
        }

        while (AudioSettings.dspTime < startDspTime) yield return null;

        double endDspTime = startDspTime + audioSource.clip.length;
        while (AudioSettings.dspTime < endDspTime) yield return null;

        FinishSong();
    }

    private void FinishSong()
    {
        if (musicFinished) return;
        musicFinished = true;

        audioSource.Stop();
        rhythmClock.Stop();
        GameEvents.RaiseMusicFinished();

#if UNITY_EDITOR
        Debug.Log("Finish playing music!");
#endif
    }
}
