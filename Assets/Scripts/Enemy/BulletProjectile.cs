
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float bulletSpeed;
    [SerializeField] private GameObject bloodFvx;


    private void Update() {
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
    }


    private void OnTriggerEnter(Collider other) {
        if(other.GetComponent<Health>()){
            Instantiate(bloodFvx, transform.position, Quaternion.identity);
            other.GetComponent<Health>().TakeDamage(5);
            Destroy(gameObject);
        }
    }
}
