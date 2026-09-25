using UnityEngine;
using System;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / Spawner")]
public class WeaponSpawnEvent : GameEvent<Vector3> { 
    [SerializeField] private string eventName = "WeaponSpawnEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to spawn a weapon";
    public string EventDescription => eventDescription;

    [SerializeField] private GameObject weaponPrefab;
    public GameObject WeaponPrefab => weaponPrefab;
    [SerializeField] private int weaponCount = 1;
    public int WeaponCount => weaponCount;

    [SerializeField] private float spawnRate = 1f;
    public float SpawnRate => spawnRate;

    [SerializeField] private float spawnRadius = 1f;
    public float SpawnRadius => spawnRadius;

    public void SpawnWeapon(Vector3 spawnLocation)
    {
        Raise(spawnLocation);
    }

}

