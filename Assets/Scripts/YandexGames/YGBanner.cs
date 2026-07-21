using UnityEngine;
using YG;

namespace YandexGames
{
    public class YGBanner : MonoBehaviour
    {
        void Start() => YG2.StickyAdActivity(true);
    }
}
