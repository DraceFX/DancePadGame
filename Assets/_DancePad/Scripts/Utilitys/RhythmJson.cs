using System;

[Serializable]
public class RhythmChartJson
{
    public string songId;
    public string title;
    public string artist;

    public double bpm = 120.0;
    public double offset = 0.0;

    public RhythmNoteJson[] notes;
}

[Serializable]
public class RhythmNoteJson
{
    public double beat;
    public string direction;
    public string type = "Tap";
}