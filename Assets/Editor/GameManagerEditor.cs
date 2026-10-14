
using UnityEngine;
using UnityEditor;

// This setting allows me to reset the game after I end the game.
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        GameManager manager = (GameManager)target;

        GUI.backgroundColor = Color.yellow;

        if (GUILayout.Button("RESET UPGRADES TO DEFAULT", GUILayout.Height(35)))
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Reset Upgrades?",
                "Reset Oxygen, Flippers, Suit, and Dodge to their default levels?",
                "Reset",
                "Cancel"
            );

            if (confirm)
            {
                Undo.RecordObject(manager, "Reset Upgrades");
                manager.ResetUpgradesToDefault();
                EditorUtility.SetDirty(manager);
            }
        }

        GUI.backgroundColor = Color.white;
    }
}