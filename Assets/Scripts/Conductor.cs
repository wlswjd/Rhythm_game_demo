using UnityEngine;

public class Conductor : MonoBehaviour
{
    public static Conductor Instance { get; private set; }

    [Header("곡 정보")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float firstBeatOffset = 0.3515f;

    [Header("참조")]
    [SerializeField] private AudioSource musicSource;

    public float SecPerBeat { get; private set; }
    public float SongPosition { get; private set; }
    public float SongPositionInBeats { get; private set; }
    public bool IsPlaying { get; private set; }

    public float CurrentBeat =>
        (float)((AudioSettings.dspTime - dspSongStartTime - firstBeatOffset) / SecPerBeat);

    private double dspSongStartTime;

    private void Awake()
    {
        Instance = this;
        SecPerBeat = 60f / bpm;
    }

    private void Start()
    {
        StartSong();
    }

    private void Update()
    {
        if (!IsPlaying) return;

        SongPosition = (float)(AudioSettings.dspTime - dspSongStartTime) - firstBeatOffset;
        SongPositionInBeats = SongPosition / SecPerBeat;
    }

    public double BeatToDspTime(float beat)
    {
        return dspSongStartTime + firstBeatOffset + beat * SecPerBeat;
    }

    public void StartSong()
    {
        dspSongStartTime = AudioSettings.dspTime;
        musicSource.Play();
        IsPlaying = true;
    }

    public void StopSong()
    {
        musicSource.Stop();
        IsPlaying = false;
    }
}