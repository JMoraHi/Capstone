
using UnityEngine;

public class PlayerHealth : Health
{

    private PlayerManager _playerManager;
    [SerializeField] private OnPlayerHPChangeEvent _onPlayerHPChange;

    private void Awake() {
        _playerManager = GetComponent<PlayerManager>();
        if (_onPlayerHPChange == null) {
            _onPlayerHPChange = Resources.Load<OnPlayerHPChangeEvent>("Scripts/SOData/Events/PlayerHealthChange");}
      
    }
    public override void Dead()
    {
        base.Dead();
        _playerManager.playerAnimation.animator.CrossFade(_playerManager.playerAnimation.DEATH_ANIM_PARAM, 0.2f);
        GameAudioManager.Instance.PlayLose();
        GameManager.Instance.GameOver(false);
        _playerManager.playerShoot.OnDead();

    }

    public override void OnChangeHealth()
    {
        base.OnChangeHealth();
        _onPlayerHPChange.Raise(CurrentHealth, maxHealth);

    }
}
