using UnityEngine;

namespace Themes
{    
    [CreateAssetMenu(fileName = "NewTheme", menuName = "New Theme")]
    public class Theme : ScriptableObject
    {
        [field: SerializeField] public Color[] blockColors { get; private set; } = new Color[10];
        [field: Header("Text")]
        [field: SerializeField] public Color primaryTextColor { get; private set; }
        [field: SerializeField] public Color secondaryTextColor { get; private set; }
        [field: SerializeField] public Color tertiaryTextColor { get; private set; }
        [field: Header("Field And Background")]
        [field: SerializeField] public Color fieldColor { get; private set; }
        [field: SerializeField] public Color backgroundColor { get; private set; }
        [field: SerializeField] public Color cellDefaultColor { get; private set; }
    }
}