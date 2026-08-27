public class RuntimeRhythmNote
{
    public DancePadDirection Direction { get; }

    public double Beat { get; }
    public double Time { get; }

    public RhythmNoteState State { get; set; }
    public RhythmNoteView View { get; set; }

    public RuntimeRhythmNote(RhythmNoteData data, double time)
    {
        Direction = data.Direction;
        Beat = data.Beat;
        Time = time;

        State = RhythmNoteState.Waiting;
        View = null;
    }

    public void Reset()
    {
        State = RhythmNoteState.Waiting;
        View = null;
    }
}
