using UnityEngine;
using YG;

public class AuthCheck : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    void Start() =>
        panel.SetActive(!YG2.player.auth);
}
