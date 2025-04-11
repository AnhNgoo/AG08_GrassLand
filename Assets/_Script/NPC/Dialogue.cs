using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/Dialogue", order = 1)]
public class Dialogue : ScriptableObject
{
    // [TextArea(3, 10)]
    public string[] sentences; // Mảng các câu thoại
}