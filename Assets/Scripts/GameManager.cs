using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

 
    [Header("Events")]
    [SerializeField] private WeaponSpawnEvent weaponSpawnEvent;
    [SerializeField] private WeaponSpawnEvent ammoSpawnEvent;
    [SerializeField] private CharacterSpawnerEvent characterSpawnerEvent;
    [SerializeField] private OnChangeAliveEnemiesEvent onChangeAliveEnemies;
    [SerializeField] private OnGameOverEvent onGameOver;
    [SerializeField] private OnAlreadyGameOverEvent onAlreadyGameOver;
    [SerializeField] private OnRestartGameEvent onRestartGame;
    [SerializeField] private OnLevelChangeEvents onLevelChangeEvents;
    [SerializeField] private OnStartNewGameEvents onStartNewGameEvents;
    [SerializeField] private OnPlayerHPChangeEvent onPlayerHPChangeEvent;
    [SerializeField] private OnAmmoChangeEvent onAmmoChangeEvent;
    [SerializeField] private CharacterSpawnerEvent _controlledCharacterSpawnEvent;
    [SerializeField] private onFollowPlayerCameraEvent _followPlayerCameraEvent;



    [Header("References")]
    [SerializeField] public WeaponSpawner weaponSpawnerPoint;
    [SerializeField] public PlayersSpawnerPoint playersTransformPoint;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float minSpawnDistance = 10f;
    // [SerializeField] private int weaponToSpawn = 10;
    // [SerializeField] private int ammoToSpawn = 10;






    [Header ("In Game Info")]
    public int totalAliveEnemies;
    public int currentAliveEnemies;
    private bool startGame = false;


    [Header("Startup")]
    [SerializeField] DifficultyDataSO easyDifficulty;
    [SerializeField] DifficultyDataSO normalDifficulty;
    [SerializeField] DifficultyDataSO hardDifficulty;
    [SerializeField] DifficultyDataSO asianDifficulty;
    
    public int defaultGameLevel = 0;
    public int currentGameLevel = 0;


    private List<Vector3> spawnedWeapons = new List<Vector3>();

//=====================================PROPERTIES==================================================

    public int CurrentAliveEnemies {
        get => currentAliveEnemies;
        set {
            currentAliveEnemies = value;
            onChangeAliveEnemies.Raise(currentAliveEnemies, totalAliveEnemies);

            if(startGame && currentAliveEnemies <= 1)
            {
                GameOver(true);  
            }
        }
    }

//==================================================================================================
    private void Awake()
    {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject);}
            else{Destroy(gameObject);}
            SceneManager.sceneLoaded += OnSceneLoaded;
            onLevelChangeEvents.OnEventRaised += HandleLevelChanges;
    }

    private void OnDestroy() {
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }



    public void GameOver(bool isPlayerWin)
    {
        onGameOver?.Raise(isPlayerWin);
        
        if(!isPlayerWin) {
            onAlreadyGameOver?.Raise(true);
        }
    }


    public void StartNewGame()
    {
        if(onStartNewGameEvents != null) {
        onStartNewGameEvents?.Raise(1);
        }else{
            Debug.Log("onStartNewGameEvents is null");
        }
    }
//====================================================================================================

    private void HandleLevelChanges(int level){
        switch (level)
        {
            case 0:
                currentGameLevel = 0;
                break;
            case 1:
                currentGameLevel = 1;
                break;
            case 2:
                currentGameLevel = 2;
                break;
            case 3:
                currentGameLevel = 3;
                break;
            default:
                currentGameLevel = 0;
                break;
        }
    }

    void HandleEasyDifficulty(DifficultyDataSO difficulty)
    {
        DoMaxWeaponSpawn(difficulty.MaxWeaponSpawn);
        DoMaxAmmoSpawn(difficulty.MaxAmmoBox);
        DoMaxCharacterSpawn(difficulty.MaxEnemyCount);
    }

    void HandleNormalDifficulty(DifficultyDataSO difficulty)
    {
        DoMaxWeaponSpawn(difficulty.MaxWeaponSpawn);
        DoMaxAmmoSpawn(difficulty.MaxAmmoBox);
        DoMaxCharacterSpawn(difficulty.MaxEnemyCount);
    }

    void HandleHardDifficulty(DifficultyDataSO difficulty)
    {
        DoMaxWeaponSpawn(difficulty.MaxWeaponSpawn);
        DoMaxAmmoSpawn(difficulty.MaxAmmoBox);
        DoMaxCharacterSpawn(difficulty.MaxEnemyCount);
    }
    void HandleAsianDifficulty(DifficultyDataSO difficulty)
    {
        DoMaxWeaponSpawn(difficulty.MaxWeaponSpawn);
        DoMaxAmmoSpawn(difficulty.MaxAmmoBox);
        DoMaxCharacterSpawn(difficulty.MaxEnemyCount);
    }

//====================================================================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1)
        {
            onLevelChangeEvents.OnEventRaised -= HandleLevelChanges;
            InstantiateObjectInGame(currentGameLevel);
            GameAudioManager.Instance.StopAudio();
            GameAudioManager.Instance.PlayInGameSoundBgm();
            currentGameLevel = 0;
        }else if (scene.buildIndex == 0) {
            GameAudioManager.Instance.StopAudio();
            GameAudioManager.Instance.PlayBgm();
        }
   
    }

    public void InstantiateObjectInGame(int currentLevel)
    {
        
        weaponSpawnerPoint = FindObjectOfType<WeaponSpawner>();
        playersTransformPoint = FindObjectOfType<PlayersSpawnerPoint>();
        if (weaponSpawnerPoint != null && playersTransformPoint != null){
            if(currentLevel == 0){HandleEasyDifficulty(easyDifficulty);}
                else if(currentLevel == 1){HandleNormalDifficulty(normalDifficulty);}
                else if(currentLevel == 2){HandleHardDifficulty(hardDifficulty);}
                else if(currentLevel == 3){HandleAsianDifficulty(asianDifficulty);}
                else{HandleEasyDifficulty(easyDifficulty);}
            

            Vector3 playerSpawnLocation = GetPlayerSpawnLocation();
            _controlledCharacterSpawnEvent.Raise(playerSpawnLocation);

            totalAliveEnemies = playersTransformPoint.GetcharList().Count;
            CurrentAliveEnemies = totalAliveEnemies;
            onChangeAliveEnemies.Raise(currentAliveEnemies, totalAliveEnemies);
        }
        startGame = true;
    }

    void DoMaxCharacterSpawn(int characterToSpawn)
    {
        for (int i = 0; i < characterToSpawn; i++)
        {
            Vector3 spawnLocation = GetPlayerSpawnLocation();
            characterSpawnerEvent.Raise(spawnLocation);
            spawnedWeapons.Add(spawnLocation);
        }
        onChangeAliveEnemies.Raise(currentAliveEnemies, totalAliveEnemies); 
    }

    void DoMaxWeaponSpawn(int weaponToSpawn)
    {
        for (int i = 0; i < weaponToSpawn; i++)
        {
            Vector3 spawnLocation = GetValidSpawnLocation();
            weaponSpawnEvent.Raise(spawnLocation);
            spawnedWeapons.Add(spawnLocation); 
        }
    }
    void DoMaxAmmoSpawn(int ammoToSpawn)
    {
        for (int i = 0; i < ammoToSpawn; i++)
        {
            Vector3 spawnLocation = GetValidSpawnLocation();
            ammoSpawnEvent.Raise(spawnLocation);
            spawnedWeapons.Add(spawnLocation); 
        }
    }

    private Vector3 GetValidSpawnLocation()
    {
        int maxAttempts = 20; 
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector3 candidate = weaponSpawnerPoint.transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius+10),
                20, 
                Random.Range(-spawnRadius, spawnRadius)
            );

            if (IsLocationValid(candidate))
                return candidate;

            attempts++;
        }

        return weaponSpawnerPoint.transform.position;
    }

    private bool IsLocationValid(Vector3 candidate)
    {
        foreach (Vector3 weaponPos in spawnedWeapons)
        {
            if (Vector3.Distance(candidate, weaponPos) < minSpawnDistance)
                return false;
       
            
        }
        return true;
    }

    private Vector3 GetPlayerSpawnLocation()
    {
        int maxAttempts = 20; 
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector3 candidate = playersTransformPoint.transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0, 
                Random.Range(-spawnRadius, spawnRadius)
            );

            if (IsPlayerLocationSpawnValid(candidate))
                return candidate;

            attempts++;
        }

        return playersTransformPoint.transform.position;
    }

    private bool IsPlayerLocationSpawnValid(Vector3 candidate)
    {
        float searchRadius = 1.0f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, searchRadius, NavMesh.AllAreas))
        {

            if ((hit.mask & (1 << NavMesh.GetAreaFromName("Walkable"))) != 0)
            {
                return true;
            }
        }
        return false;
    }


//====================================================================================================
    public void DecreaseAliveEnemies(Transform deadCharacter)
    {
        deadCharacter.parent = null;
        CurrentAliveEnemies--;
        playersTransformPoint.GetcharList().Remove(deadCharacter.gameObject);
    }
}
