
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Setting", menuName = "Player/Enemy Setting")]
public class EnemyStatsSO : ScriptableObject
{
    [SerializeField] public float moveSpeed = 3.0f;
    [SerializeField] public float startShootRange = 10.0f;
    [SerializeField] public float stopShootRange = 40.0f;

}
