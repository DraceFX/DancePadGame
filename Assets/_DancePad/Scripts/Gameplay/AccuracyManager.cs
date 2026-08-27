using UnityEngine;

public class AccuracyManager : MonoBehaviour
{
    private double earnedScore;
    private int totalNotes;

    public float Accuracy { get; private set; }

    public void RegisterResult(HitResult result)
    {
        totalNotes++;

        earnedScore += result switch
        {
            HitResult.Perfect => 1.0f,
            HitResult.Good => 0.75f,
            HitResult.Bad => 0.25f,
            HitResult.Miss => 0.0f,
            _ => 0.0f
        };

        Accuracy = totalNotes == 0 ? 100f : Mathf.Clamp((float)(earnedScore / totalNotes * 100.0f), 0f, 100f);
        GameEvents.RaiseAccuracyChanged(Accuracy);
    }

    public void Reset()
    {
        earnedScore = 0;
        totalNotes = 0;
        Accuracy = 100f;

        GameEvents.RaiseAccuracyChanged(Accuracy);
    }
}
