using System;

public static class GameEvents
{
    public static event Action<DancePadDirection> OnDancePadPressed;
    public static event Action<RuntimeRhythmNote, HitResult> OnNoteJudged;
    public static event Action<float> OnAccuracyChanged;
    public static event Action<DancePadDirection> OnDancePadPressAccepted;
    public static event Action OnMusicFinished;
    public static event Action OnStartPlay;
    public static event Action<string> OnSelectChart;
    public static event Action OnWinGame;
    public static event Action OnLoseGame;

    public static void RaiseDancePadPressed(DancePadDirection direction)
    {
        OnDancePadPressed?.Invoke(direction);
    }

    public static void RaiseNoteJudged(RuntimeRhythmNote note, HitResult result)
    {
        OnNoteJudged?.Invoke(note, result);
    }

    public static void RaiseAccuracyChanged(float accuracy)
    {
        OnAccuracyChanged?.Invoke(accuracy);
    }

    public static void RaiseDancePadPressAccepted(DancePadDirection direction)
    {
        OnDancePadPressAccepted?.Invoke(direction);
    }

    public static void RaiseMusicFinished()
    {
        OnMusicFinished?.Invoke();
    }

    public static void RaiseStartPlay()
    {
        OnStartPlay?.Invoke();
    }

    public static void RiseSelectChart(string chartId)
    {
        OnSelectChart?.Invoke(chartId);
    }

    public static void RaiseWiinGame()
    {
        OnWinGame?.Invoke();
    }

    public static void RaiseLoseGame()
    {
        OnLoseGame?.Invoke();
    }
}
