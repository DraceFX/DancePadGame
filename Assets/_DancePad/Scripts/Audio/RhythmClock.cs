using UnityEngine;

public class RhythmClock : MonoBehaviour
{
    public double StartDspTime { get; private set; }
    public bool IsRunning { get; private set; }

    public double SongTime
    {
        get
        {
            if (!IsRunning) return 0;

            return AudioSettings.dspTime - StartDspTime;
        }
    }

    public void StartAt(double startDspTime)
    {
        StartDspTime = startDspTime;
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }
}
