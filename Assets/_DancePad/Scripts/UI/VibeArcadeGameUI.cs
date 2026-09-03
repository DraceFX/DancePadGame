using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[Serializable]
public class SongInfo
{
    public string songId;
    public string title;
    public string artist;
    public int bpm;
    public int notesCount;
    public string accentColorClass; // card-cyan, card-pink, card-yellow, card-green
}

public class VibeArcadeGameUI : MonoBehaviour
{
    [Header("UIDocuments")]
    [SerializeField] private UIDocument tabletDocument;
    [SerializeField] private UIDocument tvDocument;
    [SerializeField] private AccuracyManager accuracyManager;

    // Tablet Visual Elements
    private VisualElement tabletRoot;
    private VisualElement panelLanguage;
    private VisualElement panelSongs;
    private ScrollView songsScrollView;
    private VisualElement panelDifficulty;
    private VisualElement panelRules;
    private VisualElement panelReady;
    private VisualElement panelCountdown;
    private VisualElement panelGameplay;
    private VisualElement panelWin;
    private VisualElement panelLose;

    private Label countdownLabel;
    private Label readyTrackLabel;
    private Label readyArtistLabel;
    private Label winScoreLabel;
    private Label loseScoreLabel;

    // TV Visual Elements
    private VisualElement tvRoot;
    private Label tvSongLabel;
    private Label tvScoreLabel;
    private Label tvComboLabel;
    private Label tvJudgeLabel;
    private Label tvGoalMarker;
    private Label tvPlatformHint1;
    private Label tvPlatformHint2;

    // State
    public string currentLanguage = "ru";
    private string selectedDifficulty = "HARD";
    private List<SongInfo> loadedSongs = new List<SongInfo>();
    private int selectedSongIndex = 0;
    private int currentScore = 100;

    private readonly string[] colorClasses = new string[] {
        "card-cyan", "card-pink", "card-yellow", "card-green"
    };

    private void Awake()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }

        if (tabletDocument == null) tabletDocument = GetComponent<UIDocument>();
        if (accuracyManager == null) accuracyManager = FindFirstObjectByType<AccuracyManager>();
    }

    private void OnEnable()
    {
        ScanAndLoadSongs();
        BindTabletUI();
        BindTVUI();

        // Subscribe to Game Events
        GameEvents.OnAccuracyChanged += HandleAccuracyChanged;
        GameEvents.OnMusicFinished += HandleMusicFinished;
        GameEvents.OnWinGame += HandleWinGame;
        GameEvents.OnLoseGame += HandleLoseGame;
        GameEvents.OnNoteJudged += HandleNoteJudged;
        GameEvents.OnDancePadPressed += HandleDancePadPressed;

        ResetToLanguageScreen();
    }

    private void OnDisable()
    {
        GameEvents.OnAccuracyChanged -= HandleAccuracyChanged;
        GameEvents.OnMusicFinished -= HandleMusicFinished;
        GameEvents.OnWinGame -= HandleWinGame;
        GameEvents.OnLoseGame -= HandleLoseGame;
        GameEvents.OnNoteJudged -= HandleNoteJudged;
        GameEvents.OnDancePadPressed -= HandleDancePadPressed;
    }

    private void ScanAndLoadSongs()
    {
        loadedSongs.Clear();
        string songsRoot = Path.Combine(Application.streamingAssetsPath, "Songs");

        if (!Directory.Exists(songsRoot))
        {
            Debug.LogWarning($"[Vibe Arcade] Папка песен не найдена: {songsRoot}");
            return;
        }

        string[] songDirs = Directory.GetDirectories(songsRoot);
        int colorIdx = 0;

        foreach (string dir in songDirs)
        {
            string folderName = Path.GetFileName(dir);
            SongInfo info = new SongInfo();
            info.songId = folderName;
            info.accentColorClass = colorClasses[colorIdx % colorClasses.Length];
            colorIdx++;

            string[] jsonFiles = Directory.GetFiles(dir, "*.json");
            if (jsonFiles.Length > 0)
            {
                try
                {
                    string jsonText = File.ReadAllText(jsonFiles[0]);
                    RhythmChartJson chartJson = JsonUtility.FromJson<RhythmChartJson>(jsonText);
                    if (chartJson != null)
                    {
                        info.title = chartJson.title;
                        info.artist = chartJson.artist;
                        info.bpm = Mathf.RoundToInt((float)chartJson.bpm);
                        if (chartJson.notes != null) info.notesCount = chartJson.notes.Length;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Vibe Arcade] Ошибка чтения JSON в {folderName}: {e.Message}");
                }
            }

            string[] audioFiles = Directory.GetFiles(dir, "*.*");
            foreach (string af in audioFiles)
            {
                string ext = Path.GetExtension(af).ToLowerInvariant();
                if (ext == ".mp3" || ext == ".ogg" || ext == ".wav")
                {
                    string filenameNoExt = Path.GetFileNameWithoutExtension(af);
                    if (filenameNoExt.Contains(" - "))
                    {
                        string[] parts = filenameNoExt.Split(new string[] { " - " }, StringSplitOptions.None);
                        if (string.IsNullOrEmpty(info.artist) || info.artist == "2" || info.artist == "Artist")
                            info.artist = parts[0].Trim();
                        if (string.IsNullOrEmpty(info.title) || info.title == "1" || info.title == "Title")
                            info.title = parts[1].Trim();
                    }
                    else if (string.IsNullOrEmpty(info.title))
                    {
                        info.title = filenameNoExt;
                    }
                    break;
                }
            }

            if (string.IsNullOrEmpty(info.title)) info.title = folderName;
            if (string.IsNullOrEmpty(info.artist)) info.artist = "Vibe Artist";
            if (info.bpm <= 0) info.bpm = 128;

            loadedSongs.Add(info);
        }
    }

    private void BindTabletUI()
    {
        if (tabletDocument == null) return;
        tabletRoot = tabletDocument.rootVisualElement;
        if (tabletRoot == null) return;

        panelLanguage = tabletRoot.Q<VisualElement>("panel-language");
        panelSongs = tabletRoot.Q<VisualElement>("panel-songs");
        songsScrollView = tabletRoot.Q<ScrollView>("songs-scroll-view");
        panelDifficulty = tabletRoot.Q<VisualElement>("panel-difficulty");
        panelRules = tabletRoot.Q<VisualElement>("panel-rules");
        panelReady = tabletRoot.Q<VisualElement>("panel-ready");
        panelCountdown = tabletRoot.Q<VisualElement>("panel-countdown");
        panelGameplay = tabletRoot.Q<VisualElement>("panel-gameplay");
        panelWin = tabletRoot.Q<VisualElement>("panel-win");
        panelLose = tabletRoot.Q<VisualElement>("panel-lose");

        countdownLabel = tabletRoot.Q<Label>("countdown-number");
        readyTrackLabel = tabletRoot.Q<Label>("ready-track-name");
        readyArtistLabel = tabletRoot.Q<Label>("ready-artist-name");
        winScoreLabel = tabletRoot.Q<Label>("win-score-text");
        loseScoreLabel = tabletRoot.Q<Label>("lose-score-text");

        // Language Buttons
        tabletRoot.Q<Button>("btn-lang-ru")?.RegisterCallback<ClickEvent>(evt => SetLanguage("ru"));
        tabletRoot.Q<Button>("btn-lang-en")?.RegisterCallback<ClickEvent>(evt => SetLanguage("en"));

        PopulateSongsList();

        tabletRoot.Q<Button>("btn-to-diff")?.RegisterCallback<ClickEvent>(evt => ShowPanel(panelDifficulty));

        // Difficulty Buttons
        tabletRoot.Q<Button>("diff-easy")?.RegisterCallback<ClickEvent>(evt => SelectDifficulty("EASY"));
        tabletRoot.Q<Button>("diff-med")?.RegisterCallback<ClickEvent>(evt => SelectDifficulty("MEDIUM"));
        tabletRoot.Q<Button>("diff-hard")?.RegisterCallback<ClickEvent>(evt => SelectDifficulty("HARD"));
        tabletRoot.Q<Button>("btn-to-rules")?.RegisterCallback<ClickEvent>(evt => ShowPanel(panelRules));

        // Rules & Ready
        tabletRoot.Q<Button>("btn-to-ready")?.RegisterCallback<ClickEvent>(evt => ShowPanel(panelReady));
        tabletRoot.Q<Button>("btn-start-game")?.RegisterCallback<ClickEvent>(evt => StartCountdown());

        // Test Triggers
        
        

        // Reset Buttons
        tabletRoot.Q<Button>("btn-win-reset")?.RegisterCallback<ClickEvent>(evt => ResetToLanguageScreen());
        tabletRoot.Q<Button>("btn-lose-reset")?.RegisterCallback<ClickEvent>(evt => ResetToLanguageScreen());
    }

    private void PopulateSongsList()
    {
        if (songsScrollView == null) return;
        songsScrollView.Clear();

        for (int i = 0; i < loadedSongs.Count; i++)
        {
            int idx = i;
            SongInfo song = loadedSongs[i];

            Button card = new Button();
            card.AddToClassList("arcade-card");
            card.AddToClassList(song.accentColorClass);
            card.style.height = 150;
            card.style.marginBottom = 18;

            VisualElement content = new VisualElement();
            content.AddToClassList("card-content");

            Label titleLbl = new Label(song.title);
            titleLbl.AddToClassList("card-title");
            titleLbl.style.fontSize = 40;

            Label subLbl = new Label($"{song.artist}  •  {song.bpm} BPM");
            subLbl.AddToClassList("card-subtitle");
            subLbl.style.fontSize = 24;

            content.Add(titleLbl);
            content.Add(subLbl);
            card.Add(content);

            Image arrow = new Image();
            arrow.AddToClassList("arrow-icon");
            arrow.style.backgroundImage = new StyleBackground(
                UnityEditor.AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Art/Vectors/icon_play_arrow.svg")
            );
            card.Add(arrow);

            card.RegisterCallback<ClickEvent>(evt => SelectSong(idx));
            songsScrollView.Add(card);
        }
    }

    private void BindTVUI()
    {
        if (tvDocument == null) return;
        tvRoot = tvDocument.rootVisualElement;
        if (tvRoot == null) return;

        tvSongLabel = tvRoot.Q<Label>("tv-song-name");
        tvScoreLabel = tvRoot.Q<Label>("tv-score-text");
        tvComboLabel = tvRoot.Q<Label>("tv-combo-text");
        tvJudgeLabel = tvRoot.Q<Label>("tv-judge-text");
        tvGoalMarker = tvRoot.Q<Label>("tv-goal-marker");
        tvPlatformHint1 = tvRoot.Q<Label>("tv-platform-hint-1");
        tvPlatformHint2 = tvRoot.Q<Label>("tv-platform-hint-2");
    }

    public void SetLanguage(string langCode)
    {
        currentLanguage = langCode;

        // 1. Unity Localization if enabled
        if (LocalizationSettings.AvailableLocales != null)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(langCode);
            if (locale != null) LocalizationSettings.SelectedLocale = locale;
        }

        // 2. Instant UI Toolkit text translation across all screens
        ApplyLanguageLocalization(langCode);

        ShowPanel(panelSongs);
    }

    private void ApplyLanguageLocalization(string lang)
    {
        bool isEn = lang.ToLowerInvariant() == "en";

        if (tabletRoot != null)
        {
            // Header
            SetText("brand-title", isEn ? "VIBE" : "ВАЙБ");
            SetText("brand-slogan", isEn ? "CHEAT-CODE FOR HUNGER" : "ЧИТ-КОД ОТ ГОЛОДА");

            // Songs Panel
            SetText("songs-heading", isEn ? "SELECT TRACK" : "ВЫБОР ТРЕКА");
            SetText("songs-subheading", isEn ? "CHOOSE A SONG TO PLAY" : "ВЫБЕРИТЕ ПЕСНЮ ДЛЯ ИГРЫ");
            SetText("btn-to-diff-text", isEn ? "CONTINUE ▶" : "ПРОДОЛЖИТЬ ▶");

            // Difficulty Panel
            SetText("diff-heading", isEn ? "SELECT DIFFICULTY" : "ВЫБОР СЛОЖНОСТИ");
            SetText("diff-subheading", isEn ? "CHOOSE TEMPO FOR PLATFORM" : "ВЫБЕРИТЕ ТЕМП ДЛЯ ПЛАТФОРМЫ");
            SetText("diff-easy-title", isEn ? "EASY" : "EASY (ЛЁГКИЙ)");
            SetText("diff-easy-sub", isEn ? "Calm tempo, basic arrows" : "Спокойный темп, базовые стрелки");
            SetText("diff-med-title", isEn ? "MEDIUM" : "MEDIUM (СРЕДНИЙ)");
            SetText("diff-med-sub", isEn ? "Classic arcade tempo" : "Классический темп автомата");
            SetText("diff-hard-title", isEn ? "HARD" : "HARD (ХАРДКОР)");
            SetText("diff-hard-sub", isEn ? "Maximum combo speed" : "Максимальная скорость комбо");
            SetText("btn-to-rules-text", isEn ? "TO RULES ▶" : "К ПРАВИЛАМ ▶");

            // Rules Panel
            SetText("rules-heading", isEn ? "HOW TO PLAY" : "КАК ИГРАТЬ");
            SetText("rules-step-1", isEn ? "1. Step onto the dance platform" : "1. Встаньте на танцевальную платформу");
            SetText("rules-step-2", isEn ? "2. Look at the upper TV screen" : "2. Смотрите на верхний ТВ-экран");
            SetText("rules-step-3", isEn ? "3. Step on the arrows on the beat" : "3. Наступайте на стрелки точно в такт музыке");
            SetText("rules-goal-title", isEn ? "GOAL — CHEAT-CODE FOR HUNGER:" : "ЦЕЛЬ — ЧИТ-КОД ОТ ГОЛОДА:");
            SetText("rules-goal-text", isEn ? "SCORE 65% OR MORE ACCURACY TO UNLOCK A SANDWICH FROM THE CLAW!" : "НАБЕРИТЕ ОТ 65% ТОЧНОСТИ, ЧТОБЫ РАЗБЛОКИРОВАТЬ СЭНДВИЧ ИЗ КЛЕШНИ!");
            SetText("btn-to-ready-text", isEn ? "GOT IT, NEXT ▶" : "ПОНЯТНО, ДАЛЬШЕ ▶");

            // Ready Panel
            SetText("ready-heading", isEn ? "READY TO PLAY?" : "ГОТОВЫ К ИГРЕ?");
            SetText("btn-start-game-text", isEn ? "START GAME ▶" : "НАЧАТЬ ИГРУ ▶");

            // Countdown Panel
            SetText("countdown-heading-1", isEn ? "STEP ON PLATFORM" : "ВСТАНЬТЕ НА ПЛАТФОРМУ");
            SetText("countdown-heading-2", isEn ? "AND LOOK UP!" : "И СМОТРИТЕ НАВЕРХ!");

            // Gameplay Panel
            SetText("gameplay-heading", isEn ? "GAME IN PROGRESS" : "ИДЁТ ИГРА");
            SetText("gameplay-subheading", isEn ? "LOOK AT UPPER TV SCREEN!" : "СМОТРИТЕ НА ВЕРХНИЙ ТВ-ЭКРАН!");
            SetText("gameplay-hint", isEn ? "STEP ON ARROWS TO THE BEAT!" : "НАСТУПАЙТЕ НА СТРЕЛКИ В ТАКТ МУЗЫКЕ!");

            // Win Panel
            SetText("win-heading", isEn ? "VICTORY!" : "ПОБЕДА!");
            SetText("win-subheading-1", isEn ? "CHEAT-CODE ACTIVATED!" : "ЧИТ-КОД АКТИВИРОВАН!");
            SetText("win-subheading-2", isEn ? "CLAW ACTIVATED — CLAIM YOUR SANDWICH!" : "КЛЕШНЯ АКТИВИРОВАНА — ЗАБЕРИТЕ СЭНДВИЧ!");
            SetText("btn-win-reset-text", isEn ? "COMPLETE" : "ЗАВЕРШИТЬ / COMPLETE");

            // Lose Panel
            SetText("lose-heading", isEn ? "ALMOST!" : "ПОЧТИ!");
            SetText("lose-subheading", isEn ? "NEEDED ≥65% ACCURACY TO WIN" : "ДЛЯ ПОБЕДЫ НУЖНО БЫЛО ≥65%");
            SetText("btn-lose-reset-text", isEn ? "TRY AGAIN" : "ПОПРОБОВАТЬ СНОВА");
        }

        if (tvRoot != null)
        {
            if (tvGoalMarker != null) tvGoalMarker.text = isEn ? "▲ 65% WIN GOAL (CLAW UNLOCK)" : "▲ 65% WIN GOAL (РАЗБЛОКИРОВКА КЛЕШНИ)";
            if (tvPlatformHint1 != null) tvPlatformHint1.text = isEn ? "DANCE PLATFORM" : "ТАНЦЕВАЛЬНАЯ ПЛАТФОРМА";
            if (tvPlatformHint2 != null) tvPlatformHint2.text = isEn ? "STEP ON ARROWS TO THE BEAT" : "НАСТУПАЙТЕ НА СТРЕЛКИ В ТАКТ";
        }

        UpdateReadyScreen();
    }

    private void SetText(string elementName, string text)
    {
        var lbl = tabletRoot?.Q<Label>(elementName);
        if (lbl != null) lbl.text = text;
    }

    public void SelectDifficulty(string diff)
    {
        selectedDifficulty = diff;
        UpdateReadyScreen();
        ShowPanel(panelRules);
    }

    private void UpdateReadyScreen()
    {
        if (selectedSongIndex >= 0 && selectedSongIndex < loadedSongs.Count)
        {
            SongInfo song = loadedSongs[selectedSongIndex];
            if (readyTrackLabel != null) readyTrackLabel.text = song.title;
            if (readyArtistLabel != null) readyArtistLabel.text = $"{song.artist}  •  {selectedDifficulty}  •  {song.bpm} BPM";
            if (tvSongLabel != null) tvSongLabel.text = $"{song.title} — {song.artist}";
        }
    }

    public void SelectSong(int index)
    {
        if (index < 0 || index >= loadedSongs.Count) return;

        selectedSongIndex = index;
        SongInfo song = loadedSongs[index];

        GameEvents.RiseSelectChart(song.songId);
        UpdateReadyScreen();
        ShowPanel(panelDifficulty);
    }

    public void StartCountdown()
    {
        ShowPanel(panelCountdown);
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        if (countdownLabel != null) countdownLabel.text = "3";
        yield return new WaitForSeconds(1.0f);
        if (countdownLabel != null) countdownLabel.text = "2";
        yield return new WaitForSeconds(1.0f);
        if (countdownLabel != null) countdownLabel.text = "1";
        yield return new WaitForSeconds(1.0f);
        if (countdownLabel != null) countdownLabel.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        ShowPanel(panelGameplay);
        GameEvents.RaiseStartPlay();
    }

    private void HandleAccuracyChanged(float accuracy)
    {
        currentScore = Mathf.RoundToInt(accuracy);
        if (tvScoreLabel != null)
        {
            tvScoreLabel.text = $"{currentScore}%";
        }
    }

    private void HandleNoteJudged(RuntimeRhythmNote note, HitResult result)
    {
        if (tvJudgeLabel != null)
        {
            tvJudgeLabel.text = result.ToString().ToUpper();
            tvJudgeLabel.style.color = result == HitResult.Perfect ? new Color(0, 1, 0.4f) : new Color(0, 0.94f, 1f);
        }
    }

    private void HandleDancePadPressed(DancePadDirection direction)
    {
        // Highlight corresponding visual target on TV
    }

    private void HandleMusicFinished()
    {
        // Handled via OnWinGame / OnLoseGame
    }

    private void HandleWinGame()
    {
        if (accuracyManager != null) currentScore = accuracyManager.Accuracy;
        if (winScoreLabel != null) winScoreLabel.text = $"{currentScore}%";
        ShowPanel(panelWin);
    }

    private void HandleLoseGame()
    {
        if (accuracyManager != null) currentScore = accuracyManager.Accuracy;
        if (loseScoreLabel != null) loseScoreLabel.text = $"{currentScore}%";
        ShowPanel(panelLose);
    }

    public void ResetToLanguageScreen()
    {
        ShowPanel(panelLanguage);
        currentScore = 100;
        if (tvScoreLabel != null) tvScoreLabel.text = "100%";
    }

    private void ShowPanel(VisualElement target)
    {
        if (panelLanguage != null) panelLanguage.AddToClassList("panel-hidden");
        if (panelSongs != null) panelSongs.AddToClassList("panel-hidden");
        if (panelDifficulty != null) panelDifficulty.AddToClassList("panel-hidden");
        if (panelRules != null) panelRules.AddToClassList("panel-hidden");
        if (panelReady != null) panelReady.AddToClassList("panel-hidden");
        if (panelCountdown != null) panelCountdown.AddToClassList("panel-hidden");
        if (panelGameplay != null) panelGameplay.AddToClassList("panel-hidden");
        if (panelWin != null) panelWin.AddToClassList("panel-hidden");
        if (panelLose != null) panelLose.AddToClassList("panel-hidden");

        if (target != null)
        {
            target.RemoveFromClassList("panel-hidden");
        }
    }
}
