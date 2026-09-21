using UnityEngine;

[CreateAssetMenu(fileName = "NewPattern", menuName = "Game/Beat Pattern")]
public class BeatPattern : ScriptableObject
{
    [Tooltip("이 패턴이 차지하는 마디 길이(박). 4/4 기준 4")]
    [SerializeField] private int lengthInBeats = 4;

    [Tooltip("마디 시작점 기준 노트 위치(박). 0 = 첫 박, 2 = 세 번째 박")]
    [SerializeField] private float[] noteBeats = { 0f, 2f };

    public int LengthInBeats => lengthInBeats;
    public float[] NoteBeats => noteBeats;
}