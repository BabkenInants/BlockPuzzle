using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitter : MonoBehaviour
{
    [SerializeField] private Settings settings;
    private Camera _cam;
    private Vector2Int _lastResolution;

    private void Awake() => _cam = GetComponent<Camera>();

    private void Start() => Fit();

    private void Update()
    {
        var currentResolution = new Vector2Int(Screen.width, Screen.height);
        if (currentResolution == _lastResolution) return;
        _lastResolution = currentResolution;
        Fit();
    }

    private void Fit()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = settings.height / 2;
        float horizontalSize = settings.width / aspectRatio / 2;
        _cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
        _cam.transform.position = settings.camCenter;
    }
}