
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
            if(other.TryGetComponent(out EnemyController enemyController)){
                enemyController.SearchForPlayer();
            }
            Destroy(gameObject);
        }
    }
}
