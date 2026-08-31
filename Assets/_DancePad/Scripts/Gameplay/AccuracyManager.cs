using UnityEngine;

public class AccuracyManager : MonoBehaviour
{
    private int accurateHits; // C
    private int totalNotes; // N

    public int Accuracy { get; private set; } // целое число процентов

    public void RegisterResult(HitResult result)
    {
        totalNotes++;

        if (result == HitResult.Perfect || result == HitResult.Good)
        {
            accurateHits++;
        }
        Accuracy = totalNotes == 0 ? 100 : Mathf.RoundToInt((float)accurateHits / totalNotes * 100f);


        Accuracy = Mathf.Clamp(Accuracy, 0, 100);
        GameEvents.RaiseAccuracyChanged(Accuracy);
    }

    public void Reset()
    {
        accurateHits = 0;
        totalNotes = 0;
        Accuracy = 100;

        GameEvents.RaiseAccuracyChanged(Accuracy);
    }
}
