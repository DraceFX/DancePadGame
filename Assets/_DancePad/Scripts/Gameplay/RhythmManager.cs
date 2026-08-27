using System;
using System.Collections.Generic;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmClock clock;
    [SerializeField] private RhythmJudgementSettings judgementSettings;
    [SerializeField] private AccuracyManager accuracyManager;
    [SerializeField] private RhythmNoteSpawner noteSpawner;

    private readonly List<RuntimeRhythmNote> notes = new List<RuntimeRhythmNote>();

    private int nextSpawnIndex;

    public void Initialize(RhythmChartData chart)
    {
        notes.Clear();

        foreach (var data in chart.Notes)
        {
            double time = chart.BeatToSeconds(data.Beat);
            RuntimeRhythmNote runtimeNote = new RuntimeRhythmNote(data, time);

            notes.Add(runtimeNote);
        }

        notes.Sort((a, b) => a.Time.CompareTo(b.Time));
        nextSpawnIndex = 0;

        noteSpawner.InitializePool(notes.Count);
        accuracyManager.Reset();
    }

    private void Update()
    {
        if (notes.Count == 0) return;

        double currentTime = clock.SongTime;

        ProcessMisses(currentTime);
        ProcessSpawing(currentTime);
    }

    private void ProcessMisses(double currentTime)
    {
        foreach (RuntimeRhythmNote note in notes)
        {
            if (note.State == RhythmNoteState.Waiting)
            {
                if (currentTime > note.Time + judgementSettings.BadSeconds)
                {
                    JudgeWithoutView(note, HitResult.Miss);
                }
                continue;
            }

            if (note.State != RhythmNoteState.Spawned)
            {
                continue;
            }

            if (currentTime > note.Time + judgementSettings.BadSeconds)
            {
                Judge(note, HitResult.Miss);
            }
        }
    }

    private void ProcessSpawing(double currentTime)
    {
        while (nextSpawnIndex < notes.Count)
        {
            RuntimeRhythmNote note = notes[nextSpawnIndex];
            if (!noteSpawner.ShouldSpawn(note.Time, currentTime)) break;

            SpawnNote(note);
            nextSpawnIndex++;
        }
    }

    private void SpawnNote(RuntimeRhythmNote note)
    {
        if (note.State != RhythmNoteState.Waiting) return;

        RhythmNoteView view = noteSpawner.Spawn(note);
        if (view == null) return;

        note.View = view;
        note.State = RhythmNoteState.Spawned;
    }

    private void OnEnable()
    {
        GameEvents.OnDancePadPressed += OnDancePadPressed;
    }

    private void OnDisable()
    {
        GameEvents.OnDancePadPressed -= OnDancePadPressed;
    }

    private void OnDancePadPressed(DancePadDirection direction)
    {
        // Debug.Log($"RhythmManager received: {direction}");

        double currentTime = clock.SongTime;
        RuntimeRhythmNote note = FindCandidate(direction, currentTime);
        if (note == null)
        {
            // Debug.Log($"No candidate note for {direction}");
            return;
        }
        GameEvents.RaiseDancePadPressAccepted(direction);

        double delta = Math.Abs(note.Time - currentTime);
        HitResult result = GetHitResult(delta);

        Judge(note, result);
    }

    private RuntimeRhythmNote FindCandidate(DancePadDirection direction, double currentTime)
    {
        RuntimeRhythmNote closest = null;
        double closestDistance = double.MaxValue;

        foreach (var note in notes)
        {
            if (note.State != RhythmNoteState.Spawned) continue;
            if (note.Direction != direction) continue;

            double distance = Math.Abs(note.Time - currentTime);
            if (distance > judgementSettings.BadSeconds) continue;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = note;
            }
        }

        return closest;
    }

    private HitResult GetHitResult(double delta)
    {
        if (delta <= judgementSettings.PerfectSeconds) return HitResult.Perfect;
        if (delta <= judgementSettings.GoodSeconds) return HitResult.Good;
        if (delta <= judgementSettings.BadSeconds) return HitResult.Bad;

        return HitResult.Miss;
    }

    private void Judge(RuntimeRhythmNote note, HitResult result)
    {
        if (note.State != RhythmNoteState.Spawned) return;

        if (result == HitResult.Miss)
        {
            note.State = RhythmNoteState.Missed;
            note.View?.OnMiss();

            ReturnNoteToPool(note);
        }
        else
        {
            note.State = RhythmNoteState.Hit;
            note.View?.OnHit(result);

            ReturnNoteToPool(note);
        }

        accuracyManager.RegisterResult(result);
        GameEvents.RaiseNoteJudged(note, result);
    }

    private void ReturnNoteToPool(RuntimeRhythmNote note)
    {
        if (note.View == null) return;

        RhythmNoteView view = note.View;
        note.View = null;
        noteSpawner.ReturnToPool(view);
    }

    private void JudgeWithoutView(RuntimeRhythmNote note, HitResult result)
    {
        if (note.State != RhythmNoteState.Waiting) return;

        note.State = RhythmNoteState.Missed;
        accuracyManager.RegisterResult(result);
        GameEvents.RaiseNoteJudged(note, result);
    }
}
