using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class RhythmSongLoader
{
    private const string SongsFolder = "Songs";
    private const string ChartExtension = ".json";

    private static readonly string[] AudioExtensions =
   {
        ".mp3",
        ".ogg",
        ".wav"
    };

    public static string GetSongFolder(string songId)
    {
        return Path.Combine(Application.streamingAssetsPath, SongsFolder, songId);
    }

    public static string FindChartFile(string songId)
    {
        string songFolder = GetSongFolder(songId);

        if (!Directory.Exists(songFolder))
        {
            Debug.LogError($"Song folder not found: {songFolder}");
            return null;
        }

        string[] files = Directory.GetFiles(songFolder, "*" + ChartExtension);

        if (files.Length == 0)
        {
            Debug.LogError($"No JSON chart found in: {songFolder}");
            return null;
        }

        if (files.Length > 1)
        {
            Debug.LogError($"Multiple JSON charts found in: " + $"{songFolder}. " + $"Expected exactly one.");
            return null;
        }

        return files[0];
    }

    public static string FindAudioFile(string songId)
    {
        string songFolder = GetSongFolder(songId);
        if (!Directory.Exists(songFolder))
        {
            Debug.LogError($"Song folder not found: {songFolder}");
            return null;
        }

        string[] files = Directory.GetFiles(songFolder, "*.*");

        foreach (string file in files)
        {
            string extension = Path.GetExtension(file).ToLowerInvariant();

            foreach (string supportedExtension in AudioExtensions)
            {
                if (extension == supportedExtension) return file;
            }
        }

        Debug.LogError($"No supported audio file found in: " + $"{songFolder}");
        return null;
    }

    public static RhythmChartData LoadChart(string songId)
    {
        string chartPath = FindChartFile(songId);
        if (string.IsNullOrEmpty(chartPath)) return null;

        return RhythmChartJsonLoader.LoadFromFile(chartPath);
    }
}

public static class RhythmAudioLoader
{
    public static IEnumerator Load(string path, Action<AudioClip> onLoaded)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Audio file not found: {path}");

            onLoaded?.Invoke(null);
            yield break;
        }

        string url = "file://" + path.Replace("\\", "/");
        AudioType audioType = GetAudioType(path);

        if (audioType == AudioType.UNKNOWN)
        {
            Debug.LogError($"Unsupported audio format: {path}");

            onLoaded?.Invoke(null);
            yield break;
        }

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load audio: " + $"{request.error}\n" + $"Path: {path}");

            onLoaded?.Invoke(null);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            Debug.LogError($"AudioClip is null: {path}");

            onLoaded?.Invoke(null);
            yield break;
        }

        clip.name = Path.GetFileNameWithoutExtension(path);

        onLoaded?.Invoke(clip);
    }

    private static AudioType GetAudioType(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            ".wav" => AudioType.WAV,

            _ => AudioType.UNKNOWN
        };
    }
}

