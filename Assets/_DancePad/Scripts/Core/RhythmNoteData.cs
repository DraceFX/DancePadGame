using System;
using UnityEngine;

[Serializable]
public class RhythmNoteData
{
    public DancePadDirection Direction;
    [Min(0)] public double Beat;
    public RhythmType Type = RhythmType.Tap;
}

public enum RhythmType
{
    Tap,
    Hold
}
