
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private PlayerSettingsSO playerSettings;

    [Header("Infos")]
    [SerializeField] private float turnSmoothVelocity;
    private Vector3 playerVelocity;
    public Vector2 mouseCamRotation;


    [Header("References")]
    [SerializeField] public Transform cameraTransform;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask GroundLayer;
    
    
    
    private PlayerManager _playerManager;
    private CharacterController _charController;

    private void Awake() {
        _charController = GetComponent<CharacterController>();
        _playerManager = GetComponent<PlayerManager>();
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update() {
        if(_playerManager.playerHealth.isDead) return;
        Movement();
    }

    private bool isGrounded(){
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, GroundLayer);
    }
//==================================================
    void Movement(){
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0, z).normalized;

        float moveAnim = new Vector2(x, z).sqrMagnitude;
        _playerManager.playerAnimation.animator.SetFloat(_playerManager.playerAnimation.MOVE_PARAM, moveAnim, 0.1f, Time.deltaTime);

        bool hasPlayerInputMoving = move.magnitude > 0.1f;

        if(_playerManager.playerShoot.isAiming){
            if(hasPlayerInputMoving){
                MoveCharacter(AimMovement(move));
            }
            MouseCamRotation();
        }else{
            if(hasPlayerInputMoving){
                MoveCharacter(FreeMovement(move));
            }
        }


        if(isGrounded()){
            playerVelocity.y = -2;
        }
        playerVelocity.y -= playerSettings.gravity * Time.deltaTime;
        _charController.Move(playerVelocity * Time.deltaTime);
    }
//==================================================
    // AIM
    Vector3 AimMovement(Vector3 move){
        move = transform.TransformDirection(move);
        return move;
    }

    //Free Look
    Vector3 FreeMovement(Vector3 move){
        float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg +cameraTransform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, playerSettings.turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        return moveDir;
    }
//==================================================
    void MoveCharacter(Vector3 move){
        _charController.Move(move.normalized * playerSettings.moveSpeed * Time.deltaTime);
    }

    void MouseCamRotation() {
        mouseCamRotation.x += Input.GetAxis("Mouse X") * playerSettings.mouseCamRotationSpeed ;
        mouseCamRotation.y += Input.GetAxis("Mouse Y") * playerSettings.mouseCamRotationSpeed ;

        mouseCamRotation.y = Mathf.Clamp(mouseCamRotation.y, -70f, 70f);
        transform.rotation = Quaternion.Euler(-mouseCamRotation.y, mouseCamRotation.x, 0f);
    }
//==================================================

 




//==================================================

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
