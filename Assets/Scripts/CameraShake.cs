using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Settings settings;
    private IEnumerator _coroutine = null;

    public void ShakeForSeconds(float duration, bool heavy)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            transform.position = settings.camCenter;
        }
        _coroutine = ShakeForSecondsRoutine(duration, heavy);
        StartCoroutine(_coroutine);
    }

    private IEnumerator ShakeForSecondsRoutine(float duration, bool heavy)
    {
        var elapsedTime = 0f;
        var direction = 1;
        var backToCenter = false;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            if (backToCenter)
            {
                backToCenter = false;
                transform.position = settings.camCenter;
                yield return null;
                continue;
            }
            backToCenter = true;
            direction *= -1;
            float deltaX = Random.Range(heavy? settings.minDist * 2 : settings.minDist, heavy? settings.maxDist * 2 : settings.maxDist) * direction;
            float deltaY = Random.Range(heavy? settings.minDist * 2 : settings.minDist, heavy? settings.maxDist * 2 : settings.maxDist) * direction;
            transform.position += new Vector3(deltaX, deltaY, 0);
            yield return null;
        }

        transform.position = settings.camCenter;
        _coroutine = null;
    }
}
