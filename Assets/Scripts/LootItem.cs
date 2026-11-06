
using UnityEngine;

public enum ItemType{
    Weapon,
    Consumable,
    Ammo
}

public enum WeaponType{
    RIFLE,
    PISTOL,
    SHOTGUN,
}

public class LootItem : MonoBehaviour
{
    public ItemStatSO itemStat;

    private void OnTriggerEnter(Collider other) {
        if(other.GetComponent<CharTag>()){
            if(other.TryGetComponent(out PlayerShoot playerShoot)){
                playerShoot.OnGettingItems(this.itemStat);
                
            }
            //if(other.TryGetComponent(out EnemyController enemyController)){ //Original Script
            //    enemyController.SearchForPlayer();
            //}

            // Use our new AI when the character has it.
            if (other.TryGetComponent(out MyEnemyAI myEnemyAI))
            {
                // Our AI currently collects only weapons.
                if (itemStat == null || itemStat.itemType != ItemType.Weapon)
                    return;

                // Remember the weapon and show the rifle.
                myEnemyAI.OnWeaponCollected();
            }
            // Keep the original behavior for enemies without our new AI.
            else if (other.TryGetComponent(out EnemyController enemyController))
            {
                enemyController.SearchForPlayer();
            }
            Destroy(gameObject);
        }
    }
}
