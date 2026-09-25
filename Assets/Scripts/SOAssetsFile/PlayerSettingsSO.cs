using UnityEngine;

[CreateAssetMenu(fileName = "New Player Setting", menuName = "Player/Player Setting")]
public class PlayerSettingsSO : ScriptableObject
{
    [Header("Player Stats")]
    public float moveSpeed = 10.0f;
    public float gravity = 9.81f;
    public float turnSmoothTime = 0.1f;
    public float mouseCamRotationSpeed;

   
}
