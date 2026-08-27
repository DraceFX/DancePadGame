using System;

public static class GameEvents
{
    public static event Action<DancePadDirection> OnDancePadPressed;
    public static event Action<RuntimeRhythmNote, HitResult> OnNoteJudged;
    public static event Action<float> OnAccuracyChanged;
    public static event Action<DancePadDirection> OnDancePadPressAccepted;

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
}
