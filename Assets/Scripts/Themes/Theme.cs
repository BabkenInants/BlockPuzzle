using UnityEngine;

namespace Themes
{    
    [CreateAssetMenu(fileName = "NewTheme", menuName = "New Theme")]
    public class Theme : ScriptableObject
    {
        [Tooltip("Strictly 10 Colors")]
        [field: SerializeField] public Color[] blockColors { get; private set; } = new Color[10];
        [field: SerializeField] public Color fieldColor { get; private set; }
        [field: SerializeField] public Color backgroundColor { get; private set; }
        [field: SerializeField] public Color cellDefaultColor { get; private set; }
    }
}