
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum EnemyState {
    SEARCHING_WEAPON,
    SEARCHING_PLAYER,
    ATTACK_PLAYER,
    DEAD,
    IDLE,
}
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Refference")]
    [SerializeField] private EnemyStatsSO enemyStats;
    [SerializeField] private Transform target;
    [SerializeField] private EnemyState _enemyState;
    private Health _enemyHealth;

    [Header("Shoot Refference")]
    [SerializeField] private GameObject _muzleFlash;
    [SerializeField] private GameObject _weapon;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float fireCd;
    [SerializeField] private float defaultFireCd;


    private WeaponSpawner _weaponSpawnerParentPoint;
    private PlayersSpawnerPoint _playersTransformParentPoint;
    
    
    private EnemyManager _enemyManager;
    private NavMeshAgent _navMeshAgent;

    private void Awake() {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyManager = GetComponent<EnemyManager>();
        _navMeshAgent.speed = enemyStats.moveSpeed;
    }
    private void Start() {
        _weaponSpawnerParentPoint = GameManager.Instance.weaponSpawnerPoint;
        _playersTransformParentPoint = GameManager.Instance.playersTransformPoint;
        
    }

//====================================================================================================
    private void Update() {

        if(_enemyManager.enemyHealth.isDead) return;
        switch (_enemyState) {
            case EnemyState.SEARCHING_WEAPON:
                try {
                    target = FindNearestTarget(_weaponSpawnerParentPoint.gameObject).transform;
                    if (target != null) _navMeshAgent.SetDestination(target.position);
                } catch (System.Exception e) {
                    throw e;
                }
                break;
            case EnemyState.SEARCHING_PLAYER:
                try {
                    if (_playersTransformParentPoint.GetcharList().Count > 1) {    
                        target = FindNearestTarget(_playersTransformParentPoint.gameObject).transform;
                    }else {
                        print("Enemy WIn");
                        
                    }
                    
                    if(target != null ) {_navMeshAgent.SetDestination(target.position);} else {SetIdle();}
                    if (Vector3.Distance(transform.position, target.position) < enemyStats.startShootRange) {
                        SetAttackPlayer();
                    }
                    
                } catch (System.Exception e) {
                    throw e;
                    
                }            
                break;
            case EnemyState.ATTACK_PLAYER:
                Shoot();
                if(Vector3.Distance(transform.position, target.position) > enemyStats.stopShootRange) {
                    SearchForPlayer();
                }
                break;
            case EnemyState.DEAD:
                // Destroy(this.gameObject, 2f); 
                break;

            case EnemyState.IDLE:
                break;
        }
    }

//====================================================================================================
    GameObject FindNearestTarget( GameObject targetParent) {
        if(targetParent.GetComponent<WeaponSpawner>()) {
            List<GameObject> weaponList = _weaponSpawnerParentPoint.GetWeaponList();
            var distanceNearestTarget = Mathf.Infinity;
            GameObject nearestWeapon = null;

            foreach (GameObject weapon in weaponList) {
                if (weapon != null) {
                    var distanceCurrentTarget = (weapon.transform.position - transform.position).sqrMagnitude;
                    if (distanceCurrentTarget < distanceNearestTarget) {
                        distanceNearestTarget = distanceCurrentTarget;
                        nearestWeapon = weapon;
                    }
                }
            }
            return nearestWeapon;
        }else if(targetParent.GetComponent<PlayersSpawnerPoint>() && targetParent.GetComponent<PlayersSpawnerPoint>().GetcharList().Count > 1) {
            List<GameObject> playerList = _playersTransformParentPoint.GetcharList();
        
            var distanceNearestTarget = Mathf.Infinity;
            GameObject nearestPlayer = null;
            foreach (GameObject player in playerList) {
                if(player.TryGetComponent(out Health health)) {
                    if(health.isDead) continue;
                }
                if (player != null) {
                    if (player.transform == this.transform) continue;

                    var distanceCurrentTarget = (player.transform.position - transform.position).sqrMagnitude;
                    if (distanceCurrentTarget < distanceNearestTarget) {
                        distanceNearestTarget = distanceCurrentTarget;
                        nearestPlayer = player;
                    }
                }
            }
            return nearestPlayer;
        }
        return null;
       
    }

    void TargetIsDead(){
        _enemyHealth.onDead.RemoveListener(TargetIsDead);
        SearchForPlayer();
    }


//============================================STATE===============================================
    public void SearchForPlayer() {
        if(_enemyManager.enemyHealth.isDead) return;

        if( target != null) {
            if(target.TryGetComponent(out Health health)) {
                _enemyHealth = health;
                _enemyHealth.onDead.AddListener(TargetIsDead);
            }
        }
        _enemyState = EnemyState.SEARCHING_PLAYER;
        _weapon.SetActive(true);
        _muzleFlash.SetActive(false);
        _enemyManager.enemyAnimation.animator.CrossFade(_enemyManager.enemyAnimation.RIFLE_RUN_PARAM, 0.2f);
        _navMeshAgent.isStopped = false;
    }


    public void SetAttackPlayer() {
        if(_enemyManager.enemyHealth.isDead) return;

        if( target != null) {
            if(target.TryGetComponent(out Health health)) {
                _enemyHealth = health;
                _enemyHealth.onDead.AddListener(TargetIsDead);
            }
        }
        _enemyState = EnemyState.ATTACK_PLAYER;
        _enemyManager.enemyAnimation.animator.CrossFade(_enemyManager.enemyAnimation.FIRING_PARAM, 0.2f);
        _muzleFlash.SetActive(true);
        _navMeshAgent.isStopped = true;

}

    public void SetDead() {
        if(_enemyHealth != null) _enemyHealth.onDead.RemoveListener(TargetIsDead);
        _enemyState = EnemyState.DEAD;
        _navMeshAgent.isStopped = true;
        _muzleFlash.SetActive(false);
        _weapon.SetActive(false);
    }

    public void SetIdle(){

        _enemyState = EnemyState.IDLE;
        _enemyManager.enemyAnimation.animator.CrossFade(_enemyManager.enemyAnimation.IDLE_PARAM, 0.2f);
        _navMeshAgent.isStopped = true;
        _muzleFlash.SetActive(false);
    }

//==============================================================================================
    void Shoot (){

        transform.LookAt(target);
        fireCd -= Time.deltaTime;
        if(fireCd <= 0){
            fireCd = defaultFireCd;
            Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        }

    
    }
}
