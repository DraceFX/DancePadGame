using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChartGenerator : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip music;

    [Header("Song")]
    [SerializeField] private string songId = "generated_song";
    [SerializeField] private string title = "Generated Song";
    [SerializeField] private string artist = "Unknown Arrist";

    [Header("Timing")]
    [SerializeField, Min(1f)] private double bpm = 120;
    [SerializeField] private double offset = 0.0;
    [SerializeField] private BeatDivision beatDivision = BeatDivision.Sixteenth;

    [Header("FFT")]
    [SerializeField, Range(256, 8192)] private int fftSize = 1024;
    [SerializeField, Min(0.01f)] private float analysisWindow = 0.1f;

    [Header("Frequency Range")]
    [SerializeField] private float minFrequency = 20f;
    [SerializeField] private float maxFrequency = 16000f;

    [Header("Spectral Flux")]
    [SerializeField, Range(0.001f, 1f)] private float sensitivity = 0.12f;
    [SerializeField, Range(0.01f, 1f)] private float peakWindow = 0.15f;
    [SerializeField, Min(0.01f)] private float minimumNoteDistance = 0.08f;

    [Header("Note Density")]
    [SerializeField, Range(0f, 1f)] private float lowFluxNoteChance = 0.15f;
    [SerializeField, Range(0f, 1f)] private float mediumFluxNoteChance = 0.55f;
    [SerializeField, Range(0f, 1f)] private float highFluxNoteChance = 1.0f;

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = false;

    [Header("Output")]
    [SerializeField] private bool overwriteExisting = true;

    private readonly List<RhythmNoteData> generatedNotes = new List<RhythmNoteData>();

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    [ContextMenu("Generate Chart")]
    public void Generate()
    {
        if (music == null)
        {
            Debug.LogError("Music is not assigned.");
            return;
        }

        if (bpm <= 0)
        {
            Debug.LogError("BPM must be greater than 0.");
            return;
        }

        if (!IsPowerOfTwo(fftSize))
        {
            Debug.LogError(
                $"FFT Size must be a power of two. Current: {fftSize}");

            return;
        }

        Debug.Log(
            $"Generating chart for '{music.name}'...");

        generatedNotes.Clear();

        float[] samples = GetSamples();

        if (samples == null || samples.Length == 0)
        {
            Debug.LogError("Failed to read audio samples.");
            return;
        }

        List<AnalysisPoint> analysis =
            AnalyzeAudio(samples);

        Debug.Log(
            $"Generated {analysis.Count} spectral analysis points.");

        NormalizeFlux(analysis);

        List<AnalysisPoint> peaks =
            FindFluxPeaks(analysis);

        Debug.Log(
            $"Detected {peaks.Count} musical peaks.");

        GenerateNotes(peaks);

        Debug.Log(
            $"Generated {generatedNotes.Count} notes.");

        SaveChart();

        Debug.Log("Chart generation completed.");
    }

    private float[] GetSamples()
    {
        int sampleCount = music.samples * music.channels;
        float[] samples = new float[sampleCount];
        if (!music.GetData(samples, 0))
        {
            Debug.LogError("Failed to get audio data. " + "Check AudioClip import settings.");
            return null;
        }

        return samples;
    }

    private List<AnalysisPoint> AnalyzeAudio(float[] samples)
    {
        var result = new List<AnalysisPoint>();

        int channels = music.channels;
        int sampleRate = music.frequency;
        int windowSamples = Mathf.Max(fftSize, Mathf.RoundToInt(sampleRate * analysisWindow));

        windowSamples = GetNextPowerOfTwo(windowSamples);

        float[] previousSpectrum = new float[fftSize / 2];
        float[] real = new float[fftSize];
        float[] imaginary = new float[fftSize];
        float[] monoBuffer = new float[fftSize];

        bool hasPreviousSpectrum = false;

        for (int sample = 0; sample < samples.Length; sample += windowSamples * channels)
        {
            Array.Clear(monoBuffer, 0, monoBuffer.Length);
            int availableSamples = Mathf.Min(windowSamples, (samples.Length - sample) / channels);
            if (availableSamples <= 0) break;

            for (int i = 0; i < Mathf.Min(availableSamples, fftSize); i++)
            {
                float mono = 0f;
                for (int channel = 0; channel < channels; channel++)
                {
                    int index = sample + i * channels + channel;
                    if (index >= samples.Length) break;

                    mono += samples[index];
                }

                mono /= channels;

                float window = 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * i / (fftSize - 1)));
                monoBuffer[i] = mono * window;
            }

            Array.Clear(real, 0, real.Length);
            Array.Clear(imaginary, 0, imaginary.Length);

            for (int i = 0; i < fftSize; i++)
            {
                real[i] = monoBuffer[i];
            }

            PerformFFT(real, imaginary);

            float flux = 0f;
            int bins = fftSize / 2;
            float[] currentSpectrum = new float[bins];

            for (int i = 0; i < bins; i++)
            {
                float frequency = (float)i * sampleRate / fftSize;

                if (frequency < minFrequency || frequency > maxFrequency) continue;

                float magnitude = Mathf.Sqrt(real[i] * real[i] + imaginary[i] * imaginary[i]);

                currentSpectrum[i] = magnitude;

                if (hasPreviousSpectrum)
                {
                    float difference = magnitude - previousSpectrum[i];
                    if (difference > 0f)
                    {
                        flux += difference;
                    }
                }
            }

            double time = (double)sample / (sampleRate * channels);

            result.Add(new AnalysisPoint
            {
                Time = time,
                Flux = flux
            });

            Array.Copy(currentSpectrum, previousSpectrum, bins);
            hasPreviousSpectrum = true;
        }

        return result;
    }

    private void NormalizeFlux(List<AnalysisPoint> points)
    {
        if (points.Count == 0) return;

        float maxFlux = 0f;

        foreach (AnalysisPoint point in points)
        {
            if (point.Flux > maxFlux)
            {
                maxFlux = point.Flux;
            }
        }

        if (maxFlux <= 0f)
        {
            return;
        }

        foreach (AnalysisPoint point in points)
        {
            point.Flux = Mathf.Clamp01(point.Flux / maxFlux);
        }
    }

    private List<AnalysisPoint> FindFluxPeaks(List<AnalysisPoint> analysis)
    {
        var peaks = new List<AnalysisPoint>();
        if (analysis.Count < 3) return peaks;

        for (int i = 1; i < analysis.Count - 1; i++)
        {
            AnalysisPoint previous = analysis[i - 1];
            AnalysisPoint current = analysis[i];
            AnalysisPoint next = analysis[i + 1];

            if (current.Flux < sensitivity) continue;

            bool isPeak = current.Flux >= previous.Flux && current.Flux >= next.Flux;

            if (!isPeak) continue;
            if (IsTooCloseToPreviousPeak(peaks, current.Time)) continue;

            peaks.Add(current);
        }

        return peaks;
    }

    private bool IsTooCloseToPreviousPeak(List<AnalysisPoint> peaks, double time)
    {
        if (peaks.Count == 0) return false;

        AnalysisPoint previous = peaks[peaks.Count - 1];

        return Math.Abs(previous.Time - time) < peakWindow;
    }

    private void GenerateNotes(List<AnalysisPoint> peaks)
    {
        double step = GetBeatStep();
        double secondsPerBeat = 60.0 / bpm;
        double stepSeconds = secondsPerBeat * step;
        double lastNoteTime = -minimumNoteDistance;

        DancePadDirection previousDirection = DancePadDirection.Up;

        foreach (AnalysisPoint peak in peaks)
        {
            double beat = SecondsToBeat(peak.Time);
            if (beat < 0) continue;

            beat = Math.Round(beat / step) * step;
            double noteTime = BeatToSeconds(beat);

            if (noteTime < 0 || noteTime > music.length) continue;
            if (noteTime - lastNoteTime < minimumNoteDistance) continue;

            float chance = GetNoteChance(peak.Flux);

            if (UnityEngine.Random.value > chance) continue;

            if (generatedNotes.Count > 0)
            {
                RhythmNoteData previousNote = generatedNotes[generatedNotes.Count - 1];
                if (Math.Abs(previousNote.Beat - beat) < 0.0001) continue;
            }

            DancePadDirection direction = GetNextDirection(previousDirection);

            generatedNotes.Add(new RhythmNoteData
            {
                Beat = beat,
                Direction = direction,
                Type = RhythmType.Tap
            });

            previousDirection = direction;
            lastNoteTime = noteTime;
        }
    }

    private float GetNoteChance(float flux)
    {
        if (flux >= 0.7f) return highFluxNoteChance;

        if (flux >= 0.35f)
        {
            float t = Mathf.InverseLerp(0.35f, 0.7f, flux);
            return Mathf.Lerp(mediumFluxNoteChance, highFluxNoteChance, t);
        }

        float lowT = Mathf.InverseLerp(0.05f, 0.35f, flux);
        return Mathf.Lerp(lowFluxNoteChance, mediumFluxNoteChance, lowT);
    }

    private double SecondsToBeat(double seconds)
    {
        double secondsPerBeat = 60.0 / bpm;
        return (seconds - offset) / secondsPerBeat;
    }

    private double BeatToSeconds(double beat)
    {
        double secondsPerBeat = 60.0 / bpm;
        return beat * secondsPerBeat + offset;
    }

    private double GetBeatStep()
    {
        return 1.0 / (int)beatDivision;
    }

    private DancePadDirection GetNextDirection(DancePadDirection previous)
    {
        DancePadDirection[] directions =
        {
            DancePadDirection.Up,
            DancePadDirection.Down,
            DancePadDirection.Left,
            DancePadDirection.Right,

            DancePadDirection.UpLeft,
            DancePadDirection.UpRight,
            DancePadDirection.DownLeft,
            DancePadDirection.DownRight
        };

        DancePadDirection result;

        do
        {
            result = directions[UnityEngine.Random.Range(0, directions.Length)];

        } while (result == previous);

        return result;
    }

    private void SaveChart()
    {
        string songsFolder = Path.Combine(Application.streamingAssetsPath, "Songs");

        string songFolder = Path.Combine(songsFolder, songId);

        if (!Directory.Exists(songFolder))
        {
            Directory.CreateDirectory(songFolder);
        }

        string chartPath = Path.Combine(songFolder, $"{songId}.json");

        if (File.Exists(chartPath) && !overwriteExisting)
        {
            Debug.LogError($"Chart already exists: {chartPath}");
            return;
        }

        RhythmChartJson json = new RhythmChartJson
        {
            songId = songId,
            title = title,
            artist = artist,
            bpm = bpm,
            offset = offset,
            notes = ConvertNotes()
        };

        string jsonText = JsonUtility.ToJson(json, true);

        File.WriteAllText(chartPath, jsonText);
        Debug.Log($"Chart saved: {chartPath}");
    }

    private RhythmNoteJson[] ConvertNotes()
    {
        RhythmNoteJson[] result = new RhythmNoteJson[generatedNotes.Count];
        for (int i = 0; i < generatedNotes.Count; i++)
        {
            RhythmNoteData note = generatedNotes[i];

            result[i] = new RhythmNoteJson
            {
                beat = note.Beat,
                direction = note.Direction.ToString(),
                type = note.Type.ToString()
            };
        }

        return result;
    }

    private void PerformFFT(float[] real, float[] imaginary)
    {
        int n = real.Length;
        int j = 0;

        for (int i = 0; i < n; i++)
        {
            if (i < j)
            {
                float temp = real[i];
                real[i] = real[j];
                real[j] = temp;
                temp = imaginary[i];
                imaginary[i] = imaginary[j];
                imaginary[j] = temp;
            }

            int bit = n >> 1;
            while ((j & bit) != 0)
            {
                j ^= bit;
                bit >>= 1;
            }

            j ^= bit;
        }

        for (int length = 2; length <= n; length <<= 1)
        {
            float angle = -2f * Mathf.PI / length;
            float wReal = Mathf.Cos(angle);
            float wImaginary = Mathf.Sin(angle);

            for (int i = 0; i < n; i += length)
            {
                float currentReal = 1f;
                float currentImaginary = 0f;

                int halfLength = length / 2;

                for (int k = 0; k < halfLength; k++)
                {
                    int even = i + k;
                    int odd = even + halfLength;
                    float oddReal = real[odd] * currentReal - imaginary[odd] * currentImaginary;
                    float oddImaginary = real[odd] * currentImaginary + imaginary[odd] * currentReal;

                    real[odd] = real[even] - oddReal;
                    imaginary[odd] = imaginary[even] - oddImaginary;
                    real[even] += oddReal;
                    imaginary[even] += oddImaginary;

                    float nextReal = currentReal * wReal - currentImaginary * wImaginary;

                    currentImaginary = currentReal * wImaginary + currentImaginary * wReal;
                    currentReal = nextReal;
                }
            }
        }
    }

    private int GetNextPowerOfTwo(int value)
    {
        int result = 1;
        while (result < value)
        {
            result <<= 1;
        }

        return result;
    }

    private bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    [Serializable]
    private class AnalysisPoint
    {
        public double Time;
        public float Flux;
    }
}

public enum BeatDivision
{
    Quarter = 1,
    Eighth = 2,
    Sixteenth = 4,
    ThirtySecond = 8
}