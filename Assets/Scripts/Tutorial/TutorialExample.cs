using UnityEngine;
using Core;

namespace Tutorial
{
    [CreateAssetMenu(fileName = "Tutorial Example", menuName = "Tutorial Example")]
    public class TutorialExample : ScriptableObject
    {
        public GameObject blockPrefab;
        public GameObject previewBlockPrefab;
        [HideInInspector] public Vector3 firstCellPosition;
        public GridPos targetPos;
        [HideInInspector] public int sizeX = 8;
        [HideInInspector] public int sizeY = 8;
        ///true - free, false - busy
        [HideInInspector] public bool[] cellIsFree;
    }
}