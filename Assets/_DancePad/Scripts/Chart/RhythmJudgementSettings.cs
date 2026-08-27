using UnityEngine;

[CreateAssetMenu(fileName = "RhythmJudgementSettings", menuName = "Rhythm/Judgement Settings")]
public class RhythmJudgementSettings : ScriptableObject
{
    [Header("Windows in milliseconds")]
    [Min(0)] public float PerfectWindow = 50f;
    [Min(0)] public float GoodWindow = 100f;
    [Min(0)] public float BadWindow = 150f;

    public double PerfectSeconds => PerfectWindow / 1000.0;
    public double GoodSeconds => GoodWindow / 1000.0;
    public double BadSeconds => BadWindow / 1000.0;
}
