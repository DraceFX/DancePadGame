using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RhythmChartJsonLoader
{
    public static RhythmChartData LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Rhythm chart not found: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        return LoadFromJson(json);
    }

    public static RhythmChartData LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError("Rhythm chart JSON is empty.");
            return null;
        }

        RhythmChartJson jsonData;

        try
        {
            jsonData = JsonUtility.FromJson<RhythmChartJson>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to parse rhythm chart: " + $"{exception}");
            return null;
        }

        if (jsonData == null)
        {
            Debug.LogError("Failed to deserialize rhythm chart.");
            return null;
        }
        return Convert(jsonData);
    }

    private static RhythmChartData Convert(RhythmChartJson json)
    {
        if (json.bpm <= 0)
        {
            Debug.LogError($"Invalid BPM: {json.bpm}");
            return null;
        }

        var notes = new List<RhythmNoteData>();
        if (json.notes != null)
        {
            foreach (RhythmNoteJson jsonNote in json.notes)
            {
                if (jsonNote == null) continue;

                if (jsonNote.beat < 0)
                {
                    Debug.LogError($"Invalid beat: " + $"{jsonNote.beat}");
                    continue;
                }

                if (!Enum.TryParse(jsonNote.direction, true, out DancePadDirection direction))
                {
                    Debug.LogError($"Unknown direction: " + $"{jsonNote.direction}");
                    continue;
                }

                RhythmType type = RhythmType.Tap;
                if (!string.IsNullOrWhiteSpace(jsonNote.type))
                {
                    if (!Enum.TryParse(jsonNote.type, true, out type))
                    {
                        Debug.LogError($"Unknown rhythm type: " + $"{jsonNote.type}");
                        continue;
                    }
                }

                notes.Add(new RhythmNoteData
                {
                    Beat = jsonNote.beat,
                    Direction = direction,
                    Type = type
                });
            }
        }

        notes.Sort((a, b) => a.Beat.CompareTo(b.Beat));

        return new RhythmChartData
        {
            SongId = json.songId,
            Title = json.title,
            Artist = json.artist,

            BPM = json.bpm,
            Offset = json.offset,

            Notes = notes
        };
    }
}

public class RhythmChartData
{
    public string SongId;
    public string Title;
    public string Artist;

    public double BPM;
    public double Offset;

    public List<RhythmNoteData> Notes;

    public double BeatToSeconds(double beat)
    {
        double secondsPerBeat = 60.0 / BPM;
        return beat * secondsPerBeat + Offset;
    }

    public double SecondsToBeat(double seconds)
    {
        double secondsPerBeat = 60.0 / BPM;
        return (seconds - Offset) / secondsPerBeat;
    }
}
