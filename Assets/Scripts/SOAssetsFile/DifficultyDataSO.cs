
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyData", menuName = "Difficulty/DifficultyData")]
public class DifficultyDataSO : ScriptableObject
{
    [Header("Difficulty Data")]
    [SerializeField] private string difficultyName;
    [SerializeField] private float enemyHealthMultiplier;
    [SerializeField] private float enemyDamageMultiplier;
    [SerializeField] private float enemySpeedMultiplier;
    [SerializeField] private float enemyAttackSpeedMultiplier;
    [SerializeField] private int maxEnemyCount;
    [SerializeField] private int maxAmmoBox;
    [SerializeField] private int maxWeaponSpawn;

    public string DifficultyName => difficultyName;
    public float EnemyHealthMultiplier => enemyHealthMultiplier;
    public float EnemyDamageMultiplier => enemyDamageMultiplier;
    public float EnemySpeedMultiplier => enemySpeedMultiplier;
    public float EnemyAttackSpeedMultiplier => enemyAttackSpeedMultiplier;
    public int MaxEnemyCount => maxEnemyCount;
    public int MaxAmmoBox => maxAmmoBox;
    public int MaxWeaponSpawn => maxWeaponSpawn;

}
