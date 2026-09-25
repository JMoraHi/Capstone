
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [SerializeField] private float destroyTime;

    private void Start() {
        Destroy(gameObject, destroyTime);
    }
}
