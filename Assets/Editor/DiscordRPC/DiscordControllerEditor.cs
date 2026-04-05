using UnityEditor;

[CustomEditor(typeof(DiscordController))]
public class DiscordControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}