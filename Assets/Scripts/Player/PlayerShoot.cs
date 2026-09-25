
using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Player State")]
    public bool hasWeapon;
    public bool isAiming;

    [Header("References")]
    [SerializeField] private GameObject rifleModel;
    [SerializeField] private GameObject aimCamera;
    [SerializeField] private GameObject bulletImpactVFX;
    [SerializeField] private GameObject bloodImpactVFX;
    [SerializeField] private GameObject muzzleFlashVFX;
    [SerializeField] private Cinemachine.CinemachineFreeLook freeLookCamera;

    [Header("Player SO")]
    [SerializeField] private PlayerInventorySO playerInventorySO;
    private ItemStatSO currentItemStat;

    [Header("Events")]
    [SerializeField] private OnAmmoChangeEvent _onAmmoChange;
    

    private bool isAttackCd;
    PlayerManager _playerManager;
    private int currentAmmo;


    public int CurrentAmmo { 
        get => currentAmmo;
        set {
            if(value > playerInventorySO.MAX_ALLOWED_AMMO) value = playerInventorySO.MAX_ALLOWED_AMMO;
            currentAmmo = value;
            _onAmmoChange.Raise(currentAmmo, playerInventorySO.MAX_ALLOWED_AMMO);
        }
    }

    private void Awake() {
        _playerManager = GetComponent<PlayerManager>();
        if(_onAmmoChange == null) {
            _onAmmoChange = Resources.Load<OnAmmoChangeEvent>("Scripts/SOData/Events/PlayerAmmosChangeEvents");}
    }

    private void Update() {
        if(_playerManager.playerHealth.isDead) return;
            ShootInput();
            AimShoot();
       
    }

//================================================================================================
    void ShootInput(){
        if(!isAiming) return;
        if (Input.GetMouseButtonDown(0))
        {
            _playerManager.playerAnimation.animator.SetBool(_playerManager.playerAnimation.AIM_PARAM, true);
            _playerManager.playerAnimation.animator.CrossFade(_playerManager.playerAnimation.FIRING_PARAM, 0.2f);
        }
        if(Input.GetMouseButtonUp(0)){
            _playerManager.playerAnimation.animator.SetBool(_playerManager.playerAnimation.AIM_PARAM, false);
            muzzleFlashVFX.SetActive(false);
        }
        if(Input.GetMouseButton(0)){
            Shoot();
        }
    }

    void AimShoot(){
        if(!hasWeapon) return;

        if(Input.GetMouseButtonDown(1)){
            _playerManager.playerAnimation.animator.SetBool(_playerManager.playerAnimation.AIM_PARAM, true);
            _playerManager.playerAnimation.animator.CrossFade(_playerManager.playerAnimation.AIM_WALK_PARAM, 0.2f);
            isAiming = true;
            aimCamera.SetActive(isAiming);
            
        }else if(Input.GetMouseButtonUp(1)){
            _playerManager.playerAnimation.animator.SetBool(_playerManager.playerAnimation.AIM_PARAM, false);
            isAiming = false;
            aimCamera.SetActive(isAiming);
            muzzleFlashVFX.SetActive(false);
           
        }
    }
//================================================================================================
    void Shoot(){
        if(isAttackCd) return;
        if(CurrentAmmo <= 0){
            muzzleFlashVFX.SetActive(false);
            return;
        }

        muzzleFlashVFX.SetActive(true);
        StartCoroutine(ShootCorroutine());
        CurrentAmmo--;
        if(Physics.Raycast(_playerManager.playerController.cameraTransform.position, _playerManager.playerController.cameraTransform.forward, out RaycastHit hit, 100)){
        
            if(hit.collider.GetComponent<Health>()){
                hit.collider.GetComponent<Health>().TakeDamage(currentItemStat.WeaponDamage);
                Instantiate(bloodImpactVFX, hit.point, Quaternion.LookRotation(hit.normal));
            }else {
                Instantiate(bulletImpactVFX, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

    IEnumerator ShootCorroutine(){
        isAttackCd = true;
        yield return new WaitForSeconds(currentItemStat.WeaponFireRateCD);
        isAttackCd = false;
    }

//================================================================================================
    public void OnGettingItems(ItemStatSO itemStat){
        HandleItemsPickup(itemStat);
    }

    void HandleItemsPickup(ItemStatSO itemStat){
        if(itemStat.itemType == ItemType.Ammo){
            OnGettingAmmo(itemStat.DefaultWeaponAmmo);
            return;
        }
        if(!hasWeapon){
            switch (itemStat.weaponType){
                case WeaponType.RIFLE:
                    hasWeapon = true;
                    CurrentAmmo += itemStat.DefaultWeaponAmmo;
                    rifleModel.SetActive(true);
                    _playerManager.playerAnimation.animator.SetInteger(_playerManager.playerAnimation.WEAPON_STATE_PARAM, itemStat.itemID);
                    currentItemStat = itemStat;
                    break;
                case WeaponType.PISTOL:
                    break;
                case WeaponType.SHOTGUN:
                    break;
                default:
                    break;
            }
        }
    }

    public void OnGettingAmmo(int ammo){
        CurrentAmmo += ammo;
    }
//================================================================================================

    public void OnDead(){
        muzzleFlashVFX.SetActive(false);
        aimCamera.SetActive(false);
        isAiming = false;
    }
}
