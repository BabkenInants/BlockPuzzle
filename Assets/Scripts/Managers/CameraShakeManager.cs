using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Core;

namespace Managers
{
    public class CameraShakeManager : MonoBehaviour
    {
        [SerializeField] private Settings settings;
        private IEnumerator _coroutine;

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
            var backToCenter = true;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
                backToCenter = !backToCenter;
                if (backToCenter)
                {
                    transform.position = settings.camCenter;
                    continue;
                }
                direction *= -1;
                float deltaX = Random.Range(heavy? settings.minDist * 2 : settings.minDist, heavy? settings.maxDist * 2 : settings.maxDist) * direction;
                float deltaY = Random.Range(heavy? settings.minDist * 2 : settings.minDist, heavy? settings.maxDist * 2 : settings.maxDist) * direction;
                transform.position += new Vector3(deltaX, deltaY, 0);
            }

            transform.position = settings.camCenter;
            _coroutine = null;
        }
    }
}
