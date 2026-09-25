
using System.Collections.Generic;
using UnityEngine;

public class PlayersSpawnerPoint : MonoBehaviour
{
    [Header("Object Registry")]
    [SerializeField] private CharacterSpawnerEvent _characterSpawnerEvent;
    [SerializeField] private CharacterSpawnerEvent _controlledCharacterSpawnEvent;
    [SerializeField] private onFollowPlayerCameraEvent _followPlayerCameraEvent;
    [SerializeField] private List<GameObject> charList = new List<GameObject>();



    private void OnEnable() {
        _characterSpawnerEvent.OnEventRaised += SpawnPlayers;
        _controlledCharacterSpawnEvent.OnEventRaised += SpawnControlledPlayer;
    }
    private void OnDisable() {
        _characterSpawnerEvent.OnEventRaised -= SpawnPlayers;
        _controlledCharacterSpawnEvent.OnEventRaised -= SpawnControlledPlayer;
    }

    private void SpawnPlayers(Vector3 spawnLocation)
    {
        GameObject newChar = Instantiate(_characterSpawnerEvent.CharacterPrefab, spawnLocation, Quaternion.identity, transform);
        charList.Add(newChar);
    }

    public void SpawnControlledPlayer(Vector3 spawnLocation) {
        GameObject newChar = Instantiate(_controlledCharacterSpawnEvent.CharacterPrefab, spawnLocation, Quaternion.identity, transform);
        _followPlayerCameraEvent.Raise(newChar);
        charList.Add(newChar); 
    
    }


    public List<GameObject> GetcharList() {
        return charList;
    }

    public void RemoveCharFromList(GameObject charToRemove) {
        charList.Remove(charToRemove);
    }
}
