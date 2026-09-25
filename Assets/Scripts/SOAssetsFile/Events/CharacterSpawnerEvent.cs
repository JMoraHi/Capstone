using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / Character Spawner")]
public class CharacterSpawnerEvent : GameEvent<Vector3> { 
    [SerializeField] private string eventName = "CharacterSpawnEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to spawn a character";
    public string EventDescription => eventDescription;

    [SerializeField] private GameObject characterPrefab;
    public GameObject CharacterPrefab => characterPrefab;
    [SerializeField] private int characterCount = 1;
    public int CharacterCount => characterCount;

    [SerializeField] private float spawnRate = 1f;
    public float SpawnRate => spawnRate;

    [SerializeField] private float spawnRadius = 1f;
    public float SpawnRadius => spawnRadius;

    public void SpawnCharacter(Vector3 spawnLocation)
    {
        Raise(spawnLocation);
    }

}
