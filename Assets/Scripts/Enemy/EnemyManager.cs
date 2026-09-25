
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Refference")]
    [SerializeField] public Health enemyHealth;
    [SerializeField] public EnemyAnimation enemyAnimation;
    [SerializeField] public EnemyController enemyController;

    [Header("Public Events")]
    [SerializeField] public OnAlreadyGameOverEvent onAlreadyGameOverEvent;


    private void OnEnable() {
        onAlreadyGameOverEvent.OnEventRaised += DoAllEnemyIdle;
    }
    private void OnDisable() {
        onAlreadyGameOverEvent.OnEventRaised -= DoAllEnemyIdle;
    }

    void DoAllEnemyIdle(bool isGameOver) {
        enemyAnimation.animator.CrossFade(enemyAnimation.IDLE_PARAM, 0.2f);
        enemyController.SetIdle();
    }

    

}
