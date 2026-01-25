using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private int endlessModeBuildIndex = 1;
    [SerializeField] private HapticManager hapticManager;
    
    public void Play()
    {
        SceneManager.LoadScene(endlessModeBuildIndex);
        hapticManager.Light();
    }
}