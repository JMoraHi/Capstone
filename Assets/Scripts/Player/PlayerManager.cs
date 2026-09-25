
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Refference")]
    [SerializeField] public PlayerController playerController;
    [SerializeField] public PlayerAnimation playerAnimation;
    [SerializeField] public PlayerShoot playerShoot;
    [SerializeField] public PlayerHealth playerHealth;

    // private void Start() {
    //     GameManager.Instance.CurrentAliveEnemies += 1;
    // }
}
