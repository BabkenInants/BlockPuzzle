using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class AuthCheck : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    private bool _unsubscribe;

    private void Start() => OpenPopup();

    public void OpenPopup() => panel.SetActive(!YG2.player.auth);

    private void Auth() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void AuthButton()
    {
        if(!_unsubscribe)
        {
            YG2.onGetSDKData += Auth;
            YG2.OpenAuthDialog();
        }
        _unsubscribe = true;
        panel.SetActive(false);
    }

    private void OnDisable()
    {
        if (_unsubscribe)
            YG2.onGetSDKData -= Auth;
    }
}
