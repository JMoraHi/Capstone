
using UnityEngine;

public class EnemyHealth : Health
{
    private EnemyManager enemyManager;
    [SerializeField] private Collider AliveCollider;
    [SerializeField] private Collider DeadCollider;

    private void Awake() {
        enemyManager = GetComponent<EnemyManager>();
    }

    public override void Dead()
    {
        base.Dead();
        AliveCollider.enabled = false;
        DeadCollider.enabled = true;
        enemyManager.enemyAnimation.animator.CrossFade(enemyManager.enemyAnimation.DEATH_PARAM, 0.2f);
        enemyManager.enemyController.SetDead();
    }
}
