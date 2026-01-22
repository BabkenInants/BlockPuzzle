using UnityEngine;

public class AutoDestroyer : MonoBehaviour
{
    public float lifeTime;

    private void Start() => Destroy(gameObject, lifeTime);
}