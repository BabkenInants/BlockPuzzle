using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Block))]
public class BlockShapeEditor : Editor
{
    private const int CellSize = 25;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Block shape = (Block)target;
        shape.sizeX = EditorGUILayout.IntSlider("Matrix SizeX", shape.sizeX, 1, 8);
        shape.sizeY = EditorGUILayout.IntSlider("Matrix SizeY", shape.sizeY, 1, 8);
        if (shape.blockShape == null || shape.blockShape.Length != shape.sizeX * shape.sizeY)
            shape.blockShape = new bool[shape.sizeX * shape.sizeY];
        GUILayout.Space(10);
        
        // Draw grid
        for (int y = 0; y < shape.sizeY; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < shape.sizeX; x++)
            {
                int index = y *  shape.sizeX + x;
                bool value = shape.blockShape[index];
                GUI.backgroundColor = value ? Color.green : Color.gray;
                if (GUILayout.Button("", GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                {
                    shape.blockShape[index] = !shape.blockShape[index];
                    EditorUtility.SetDirty(shape);
                }
            }
            GUILayout.EndHorizontal();
        }
        
        GUILayout.Space(5);
        GUI.backgroundColor = Color.white;
    }
}