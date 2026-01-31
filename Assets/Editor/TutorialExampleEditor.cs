using UnityEditor;
using Tutorial;
using Unity.VisualScripting;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TutorialExample))]
    public class TutorialExampleEditor : UnityEditor.Editor
    {
        private const int CellSize = 25;
        private bool _showPositionHint;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);
            var example = (TutorialExample) target;
            example.sizeX = EditorGUILayout.IntSlider("Matrix SizeX", example.sizeX, 1, 8);
            example.sizeY = EditorGUILayout.IntSlider("Matrix SizeY", example.sizeY, 1, 8);
            if (example.cellIsFree == null || example.cellIsFree.Length != example.sizeX * example.sizeY)
            {
                example.cellIsFree = new bool[example.sizeX * example.sizeY];
                for(var i = 0; i < example.cellIsFree.Length; i++)
                    example.cellIsFree[i] = true;
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Show Position Hints");
            if (GUILayout.Button(_showPositionHint ? "Hide" : "Show"))
            {
                _showPositionHint = !_showPositionHint;
            }
            
            GUILayout.Space(10);
            if (GUILayout.Button("Reset Field"))
            {
                example.cellIsFree = new bool[example.sizeX * example.sizeY];
                for(var i = 0; i < example.cellIsFree.Length; i++)
                    example.cellIsFree[i] = true;
            }

            if (GUILayout.Button("Log Field"))
            {
                string result = "";
                for (int i = 0; i < example.sizeY; i++)
                {
                    for (int j = 0; j < example.sizeX; j++)
                        result += example.cellIsFree[i * example.sizeX + j] + ", ";
                    result += "\n";
                }
                Debug.Log(result);
            }

            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            GUILayout.Label("Example Field");
            
            //Grid
            for (var row = 0; row < example.sizeY; row++)
            {
                GUILayout.BeginHorizontal();
                for (var col = 0; col < example.sizeX; col++)
                {
                    int index = row * example.sizeX + col;
                    bool value = example.cellIsFree[index];
                    GUI.color = value ? Color.green : Color.gray;
                    if (GUILayout.Button(_showPositionHint? $"{row},{col}" : "", GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                    {
                        example.cellIsFree[index] = !value;
                        EditorUtility.SetDirty(example);
                    }
                }
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(5);
            GUI.backgroundColor = Color.white;
        }
    }
}