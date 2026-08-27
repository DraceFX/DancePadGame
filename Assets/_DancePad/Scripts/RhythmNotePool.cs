using System.Collections.Generic;
using UnityEngine;

public class RhythmNotePool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private RhythmNotePoolMode mode = RhythmNotePoolMode.EntireChart;

    [SerializeField, Min(1)] private int initialSize = 32;

    [Header("References")]
    [SerializeField] private RhythmNoteView notePrefab;

    [SerializeField] private RectTransform container;

    private readonly Queue<RhythmNoteView> available = new Queue<RhythmNoteView>();
    private readonly HashSet<RhythmNoteView> allObjects = new HashSet<RhythmNoteView>();
    private readonly HashSet<RhythmNoteView> activeObjects = new HashSet<RhythmNoteView>();

    public int TotalCount => allObjects.Count;
    public int AvailableCount => available.Count;
    public int ActiveCount => TotalCount - AvailableCount;

    public RhythmNotePoolMode Mode => mode;

    public void Initialize(int chartNoteCount)
    {
        ClearPool();
        int size = GetInitialPoolSize(chartNoteCount);
        Prewarm(size);
    }

    private int GetInitialPoolSize(int chartNoteCount)
    {
        return mode switch
        {
            RhythmNotePoolMode.EntireChart => Mathf.Max(1, chartNoteCount),
            RhythmNotePoolMode.FixedSize => Mathf.Max(1, initialSize),
            RhythmNotePoolMode.AutoExpand => Mathf.Max(1, initialSize),

            _ => Mathf.Max(1, initialSize)
        };
    }

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateObject();
        }
    }

    private RhythmNoteView CreateObject()
    {
        RhythmNoteView view = Instantiate(notePrefab, container);

        view.gameObject.SetActive(false);

        allObjects.Add(view);
        available.Enqueue(view);

        return view;
    }

    public RhythmNoteView Get()
    {
        if (available.Count == 0)
        {
            if (mode == RhythmNotePoolMode.FixedSize)
            {
                Debug.LogError("RhythmNotePool is exhausted.");
                return null;
            }

            CreateObject();
        }

        RhythmNoteView view = available.Dequeue();
        activeObjects.Add(view);
        view.gameObject.SetActive(true);

        return view;
    }

    public void Return(RhythmNoteView view)
    {
        if (view == null) return;
        if (!activeObjects.Remove(view)) return;

        view.ResetView();
        available.Enqueue(view);
    }

    private void ClearPool()
    {
        foreach (RhythmNoteView view in allObjects)
        {
            if (view != null) Destroy(view.gameObject);
        }

        allObjects.Clear();
        available.Clear();
        activeObjects.Clear();
    }
}

public enum RhythmNotePoolMode
{
    EntireChart,
    FixedSize,
    AutoExpand
}
