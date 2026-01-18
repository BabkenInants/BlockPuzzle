using UnityEngine;

public class AutoDestroyer : MonoBehaviour
{
    public float lifeTime;

    private void Awake() => Destroy(gameObject, lifeTime);
}