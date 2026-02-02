using Core;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(Block))]
    public class BlockShapeEditor : UnityEditor.Editor
    {
        private const int CellSize = 25;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var shape = (Block)target;
            shape.sizeX = EditorGUILayout.IntSlider("Matrix SizeX", shape.sizeX, 1, 8);
            shape.sizeY = EditorGUILayout.IntSlider("Matrix SizeY", shape.sizeY, 1, 8);
            
            //Resetting blockshape if the size was changed, or it wasn't initialized
            if (shape.blockShape == null || shape.blockShape.Length != shape.sizeX * shape.sizeY)
                shape.blockShape = new bool[shape.sizeX * shape.sizeY];
            GUILayout.Space(10);
        
            //Draw grid
            for (var row = 0; row < shape.sizeY; row++)
            {
                GUILayout.BeginHorizontal();
                for (var col = 0; col < shape.sizeX; col++)
                {
                    int index = row *  shape.sizeX + col;
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
}