using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RhythmBattle : MonoBehaviour
{
    private enum Judgment { Perfect, Good, Miss }

    [Header("채보")]
    [SerializeField] private BeatPattern[] patterns;
    [SerializeField] private float startBeat = 8f;
    [SerializeField] private int roundCount = 14;

    [Header("판정 윈도우 (ms)")]
    [SerializeField] private float perfectWindowMs = 50f;
    [SerializeField] private float goodWindowMs = 100f;

    [Header("제시 소리")]
    [SerializeField] private bool callGridTicks = true;
    [SerializeField] private float scheduleAheadSec = 0.2f;

    [Header("UI")]
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text judgmentText;

    [Header("승패")]
    [SerializeField, Range(0f, 1f)] private float winHitRate = 0.6f;

    [Header("결과 화면")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject retryButton;

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private struct Round
    {
        public float callStart;
        public float responseStart;
        public float end;
        public string pattern;
    }

    private struct ScheduledSound
    {
        public float beat;
        public AudioClip clip;
    }

    private class Note
    {
        public int round;
        public string pattern;
        public float beat;
        public bool judged;
    }

    private readonly List<Round> rounds = new List<Round>();
    private readonly List<Note> responseNotes = new List<Note>();
    private readonly List<ScheduledSound> scheduledSounds = new List<ScheduledSound>();
    private readonly List<string> logRows = new List<string>();

    private AudioSource[] soundSources;
    private AudioSource hitSource;
    private AudioClip callClip;
    private AudioClip tickClip;
    private AudioClip hitClip;
    private int nextSoundIndex;
    private int sourceIndex;

    private float endBeat;
    private bool finished;
    private bool logWritten;
    private string sessionId;
    private int perfectCount, goodCount, missCount, strayCount;

    private void Start()
    {
        if (patterns == null || patterns.Length == 0)
        {
            Debug.LogError("RhythmBattle: 패턴이 비어 있습니다.");
            enabled = false;
            return;
        }
        if (resultPanel != null) resultPanel.SetActive(false);
        sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        SetupAudio();
        BuildChart();
    }

    private void SetupAudio()
    {
        callClip = CreateClick("Call", 880f, 0.5f);
        tickClip = CreateClick("Tick", 440f, 0.2f);
        hitClip = CreateClick("Hit", 1320f, 0.5f);

        soundSources = new AudioSource[8];
        for (int i = 0; i < soundSources.Length; i++)
        {
            soundSources[i] = gameObject.AddComponent<AudioSource>();
            soundSources[i].playOnAwake = false;
        }

        hitSource = gameObject.AddComponent<AudioSource>();
        hitSource.playOnAwake = false;
    }

    private void BuildChart()
    {
        float cursor = startBeat;
        for (int r = 0; r < roundCount; r++)
        {
            BeatPattern p = patterns[r % patterns.Length];
            string patternName = p.name;
            float callStart = cursor;
            float responseStart = cursor + p.LengthInBeats;

            if (callGridTicks)
            {
                for (int b = 0; b < p.LengthInBeats; b++)
                {
                    scheduledSounds.Add(new ScheduledSound { beat = callStart + b, clip = tickClip });
                }
            }

            foreach (float nb in p.NoteBeats)
            {
                scheduledSounds.Add(new ScheduledSound { beat = callStart + nb, clip = callClip });
                responseNotes.Add(new Note { round = r, pattern = patternName, beat = responseStart + nb });
            }

            rounds.Add(new Round
            {
                callStart = callStart,
                responseStart = responseStart,
                end = responseStart + p.LengthInBeats,
                pattern = patternName
            });

            cursor += p.LengthInBeats * 2;
        }

        scheduledSounds.Sort((a, b) => a.beat.CompareTo(b.beat));
        endBeat = cursor;
        Debug.Log($"[CHART] {roundCount}라운드, 응답 노트 {responseNotes.Count}개, 종료 박 {endBeat}, 틱 {callGridTicks}");
    }

    private void Update()
    {
        Conductor c = Conductor.Instance;
        if (c == null || !c.IsPlaying || finished) return;

        float nowBeat = c.CurrentBeat;

        ScheduleSounds(c);
        HandleInput(c, nowBeat);
        CheckMisses(c, nowBeat);
        UpdatePhaseText(nowBeat);

        if (nowBeat > endBeat + 1f) Finish();
    }

    private void ScheduleSounds(Conductor c)
    {
        while (nextSoundIndex < scheduledSounds.Count)
        {
            ScheduledSound s = scheduledSounds[nextSoundIndex];
            double targetDsp = c.BeatToDspTime(s.beat);
            if (targetDsp - AudioSettings.dspTime > scheduleAheadSec) break;

            AudioSource src = soundSources[sourceIndex];
            sourceIndex = (sourceIndex + 1) % soundSources.Length;
            src.clip = s.clip;
            src.PlayScheduled(targetDsp);
            nextSoundIndex++;
        }
    }

    private void HandleInput(Conductor c, float nowBeat)
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || !kb.spaceKey.wasPressedThisFrame) return;

        hitSource.PlayOneShot(hitClip);

        Note best = null;
        float bestErrMs = float.MaxValue;
        foreach (Note n in responseNotes)
        {
            if (n.judged) continue;
            float errMs = (nowBeat - n.beat) * c.SecPerBeat * 1000f;
            if (Mathf.Abs(errMs) < Mathf.Abs(bestErrMs))
            {
                best = n;
                bestErrMs = errMs;
            }
        }

        if (best == null || Mathf.Abs(bestErrMs) > goodWindowMs)
        {
            strayCount++;
            int round = FindRound(nowBeat, out string phase);
            string pattern = round >= 0 ? rounds[round].pattern : "";
            AddLogRow(round, pattern, phase, "", F3(nowBeat), "Stray", "");
            Debug.Log($"[STRAY] beat {nowBeat:F3} ({phase})");
            return;
        }

        best.judged = true;
        Judgment j = Mathf.Abs(bestErrMs) <= perfectWindowMs ? Judgment.Perfect : Judgment.Good;
        if (j == Judgment.Perfect) perfectCount++; else goodCount++;

        AddLogRow(best.round, best.pattern, "PLAY", F2(best.beat), F3(nowBeat), j.ToString(), bestErrMs.ToString("F1", Inv));
        ShowJudgment($"{j.ToString().ToUpper()}  {bestErrMs:+0;-0}ms");
        Debug.Log($"[JUDGE] R{best.round} beat {best.beat:F2} {j} {bestErrMs:F1}ms");
    }

    private void CheckMisses(Conductor c, float nowBeat)
    {
        foreach (Note n in responseNotes)
        {
            if (n.judged) continue;
            float errMs = (nowBeat - n.beat) * c.SecPerBeat * 1000f;
            if (errMs > goodWindowMs)
            {
                n.judged = true;
                missCount++;
                AddLogRow(n.round, n.pattern, "PLAY", F2(n.beat), "", "Miss", "");
                ShowJudgment("MISS");
                Debug.Log($"[JUDGE] R{n.round} beat {n.beat:F2} Miss");
            }
        }
    }

    private int FindRound(float beat, out string phase)
    {
        for (int i = 0; i < rounds.Count; i++)
        {
            Round r = rounds[i];
            if (beat >= r.callStart && beat < r.responseStart) { phase = "LISTEN"; return i; }
            if (beat >= r.responseStart && beat < r.end) { phase = "PLAY"; return i; }
        }
        phase = beat < startBeat ? "READY" : "END";
        return -1;
    }

    private void UpdatePhaseText(float nowBeat)
    {
        if (phaseText == null) return;
        FindRound(nowBeat, out string phase);
        phaseText.text = phase;
    }

    private void ShowJudgment(string text)
    {
        if (judgmentText != null) judgmentText.text = text;
    }

    private void Finish()
    {
        finished = true;

        int total = responseNotes.Count;
        int hits = perfectCount + goodCount;
        float hitRate = total > 0 ? (float)hits / total : 0f;
        bool win = hitRate >= winHitRate;

        if (win) GameState.BossCleared = true;
        if (Conductor.Instance != null) Conductor.Instance.StopSong();

        if (phaseText != null) phaseText.text = win ? "CLEAR" : "FAILED";
        if (judgmentText != null) judgmentText.text = "";

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultText.text =
                $"{(win ? "CLEAR" : "FAILED")}\n\n" +
                $"Perfect  {perfectCount}\n" +
                $"Good  {goodCount}\n" +
                $"Miss  {missCount}\n\n" +
                $"Hit Rate  {hitRate * 100f:F0}%  (need {winHitRate * 100f:F0}%)";
            continueButton.SetActive(win);
            retryButton.SetActive(!win);
        }

        Debug.Log($"[RESULT] Perfect {perfectCount} / Good {goodCount} / Miss {missCount} / Stray {strayCount} / HitRate {hitRate:F3} / {(win ? "WIN" : "LOSE")}");
        WriteLog();
    }

    private void OnDestroy()
    {
        WriteLog();
    }

    private void AddLogRow(int round, string pattern, string phase, string noteBeat, string inputBeat, string result, string errMs)
    {
        logRows.Add(string.Join(",",
            sessionId,
            callGridTicks ? "1" : "0",
            round.ToString(Inv),
            pattern,
            phase,
            noteBeat,
            inputBeat,
            result,
            errMs));
    }

    private void WriteLog()
    {
        if (logWritten || logRows.Count == 0) return;
        logWritten = true;

        string dir = Path.Combine(Application.dataPath, "..", "PlayLogs");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"Judge_{sessionId}.csv");

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("session,ticks,round,pattern,phase,note_beat,input_beat,result,error_ms");
            foreach (string row in logRows) writer.WriteLine(row);
        }

        Debug.Log($"[LOG] 저장: {path} ({logRows.Count}행)");
    }

    private static string F2(float v) => v.ToString("F2", Inv);
    private static string F3(float v) => v.ToString("F3", Inv);

    private static AudioClip CreateClick(string clipName, float frequency, float amplitude)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int length = Mathf.RoundToInt(sampleRate * 0.06f);
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 50f);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * amplitude;
        }
        AudioClip clip = AudioClip.Create(clipName, length, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}