using System;
using System.Collections.Generic;
using UnityEngine;

public class RhythmNoteSpawner : MonoBehaviour
{
    [Serializable]
    public class DirectionLane
    {
        public DancePadDirection Direction;

        public RectTransform SpawnPoint;
        public RectTransform TargetPoint;
    }
    [Header("References")]
    [SerializeField] private RhythmClock clock;
    [SerializeField] private RhythmNotePool pool;

    [Header("Prefab")]
    [SerializeField] private RhythmNoteView notePrefab;
    [SerializeField] private RectTransform notesContainer;

    [Header("Movement")]
    [SerializeField] private float spawnLeadTime = 2f;

    [Header("Lanes")]
    [SerializeField] private List<DirectionLane> lanes = new List<DirectionLane>();

    private Dictionary<DancePadDirection, DirectionLane> laneDictionary;

    private void Awake()
    {
        laneDictionary = new Dictionary<DancePadDirection, DirectionLane>();

        foreach (var lane in lanes)
        {
            laneDictionary[lane.Direction] = lane;
        }
    }

    public void InitializePool(int chartNoteCount)
    {
        pool.Initialize(chartNoteCount);
    }

    public bool ShouldSpawn(double noteTime, double currentTime)
    {
        return currentTime >= noteTime - spawnLeadTime;
    }

    public RhythmNoteView Spawn(RuntimeRhythmNote note)
    {
        if (!laneDictionary.TryGetValue(note.Direction, out DirectionLane lane))
        {
            Debug.LogError($"Lane not found: {note.Direction}");
            return null;
        }

        RhythmNoteView view = pool.Get();
        if (view == null) return null;

        view.Initialize(note, clock, lane.SpawnPoint, lane.TargetPoint, spawnLeadTime);

        return view;
    }

    public void ReturnToPool(RhythmNoteView view)
    {
        if (view == null) return;
        pool.Return(view);
    }
}
