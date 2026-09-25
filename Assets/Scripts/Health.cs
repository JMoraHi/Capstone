
using System;
using UnityEngine;
using UnityEngine.Events;


public class Health : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100.0f;
    [SerializeField] private float currentHealth;
    [HideInInspector] public UnityEvent onDead = new UnityEvent();
    
    public float CurrentHealth {
        get => currentHealth;
        set {
            currentHealth = value;
            OnChangeHealth();
            if(currentHealth <= 0){
                Dead();
            }
        }
    }
    public bool isDead;



    private void Start() {
        CurrentHealth = maxHealth;
    }


    public void TakeDamage(float damage){
        if(isDead) return;
        CurrentHealth -= damage;
    }
    
    public virtual void OnChangeHealth(){
    }

    public virtual void Dead(){
        onDead?.Invoke( );
        isDead = true;
        GameManager.Instance.DecreaseAliveEnemies(this.transform);
    }

 
}
