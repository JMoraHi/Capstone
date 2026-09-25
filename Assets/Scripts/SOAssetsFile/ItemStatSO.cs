
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item")]
public class ItemStatSO : ScriptableObject
{
    [SerializeField] public string itemName;
    [SerializeField] public int itemID;
    [SerializeField] public int itemValue;
    [SerializeField] public ItemType itemType;
    [SerializeField] public WeaponType weaponType;
    [SerializeField] public float WeaponFireRateCD;
    [SerializeField] public float WeaponDamage;
    [SerializeField] public int DefaultWeaponAmmo;
    [SerializeField] public Sprite itemIcon;
    [SerializeField] public GameObject itemPrefab;
    [SerializeField] public GameObject itemWorldPrefab;

}
