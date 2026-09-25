
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Object Registry")]

    [SerializeField] private WeaponSpawnEvent weaponSpawnEvent;
    [SerializeField] private List<GameObject> weaponList = new List<GameObject>();



    private void OnEnable() {
        weaponSpawnEvent.OnEventRaised += SpawnWeapon;
    }
    private void OnDisable() {
        weaponSpawnEvent.OnEventRaised -= SpawnWeapon;
    }

    private void SpawnWeapon(Vector3 spawnLocation)
    {
        GameObject newWeapon = Instantiate(weaponSpawnEvent.WeaponPrefab, spawnLocation, Quaternion.identity, transform);
        
        weaponList.Add(newWeapon);
    }

    public List<GameObject> GetWeaponList() {
        return weaponList;
    }
}
