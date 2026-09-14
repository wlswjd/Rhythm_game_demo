using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string speakerName;
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;

    public string SpeakerName => speakerName;
    public string[] Lines => lines;
}