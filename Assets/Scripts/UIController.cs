
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] public OnPlayerHPChangeEvent _onPlayerHPChange;
    [SerializeField] public OnAmmoChangeEvent _onAmmoChange;
    [SerializeField] public OnChangeAliveEnemiesEvent _onChangeAliveEnemies;
    [SerializeField] public OnGameOverEvent _onGameOver;
    [SerializeField] public OnRestartGameEvent _onRestartGame;

    [Header("References")]
    [SerializeField] private TMPro.TextMeshProUGUI _aliveEnemiesText;
    [SerializeField] private TMPro.TextMeshProUGUI _RANK;
    [SerializeField] private TMPro.TextMeshProUGUI _AMMO_TEXT;
    [SerializeField] private Image _playerHPBar;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _menuPanel;
    public GameObject _winCanvas;
    public GameObject _loseCanvas;
    [SerializeField] public LoadingScreenBarSystem _loadingScreenBarSystem;

    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _exitButton;
    private GraphicRaycaster _graphicRaycaster;

    private void Awake() {
        _menuPanel.SetActive(false);
        _loadingScreenBarSystem.gameObject.SetActive(false);
        _graphicRaycaster = GetComponent<GraphicRaycaster>();
        _graphicRaycaster.enabled = false;
    }

    private void Start() {
        if (_onPlayerHPChange == null) {
            _onPlayerHPChange = Resources.Load<OnPlayerHPChangeEvent>("Scripts/SOData/Events/PlayerHealthChange");}
        if(_onAmmoChange == null) {
            _onAmmoChange = Resources.Load<OnAmmoChangeEvent>("Scripts/SOData/Events/PlayerAmmosChangeEvents");}
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            
            OpenMenu();
        }
    }

    private void OnEnable() {

        _onPlayerHPChange.OnChange += OnPlayerHPChange;
        _onAmmoChange.OnChange += OnAmmoChange;
        _onChangeAliveEnemies.OnChange += OnChangeAliveEnemies;
        _onGameOver.OnEventRaised += OnGameOver;
        _onRestartGame.OnEventRaised += OnRestartGame;
        
    }

    private void OnDisable() {
        _onPlayerHPChange.OnChange -= OnPlayerHPChange;
        _onChangeAliveEnemies.OnChange -= OnChangeAliveEnemies;
        _onGameOver.OnEventRaised -= OnGameOver;
        _onAmmoChange.OnChange -= OnAmmoChange;
        _onRestartGame.OnEventRaised -= OnRestartGame;
    }

    private void OnPlayerHPChange(float currentHP, float maxHP) {
        _playerHPBar.fillAmount = currentHP / maxHP;
    }

    private void OnChangeAliveEnemies(int currentAliveEnemies, int totalAliveEnemies) {
        _aliveEnemiesText.text = $"Alive : {currentAliveEnemies}/{totalAliveEnemies}";
    }

    private void OnGameOver(bool isGameOver) {
        _gameOverPanel.SetActive(true);
     
        if(isGameOver){ 
            _RANK.text = $"RANK: <color=green>1</color> / {GameManager.Instance.totalAliveEnemies}";
            _winCanvas.SetActive(true);
            _loseCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _graphicRaycaster.enabled = true;
        }else {
            _winCanvas.SetActive(false);
            _graphicRaycaster.enabled = true;
            _RANK.text = $"RANK: <color=red>{GameManager.Instance.CurrentAliveEnemies + 1}</color>  / {GameManager.Instance.totalAliveEnemies}";
            _loseCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnAmmoChange(int currentAmmo, int maxAmmo) {
        _AMMO_TEXT.text = $"{currentAmmo}/{maxAmmo}";
    }

    private void OnRestartGame(int value) {
        _loadingScreenBarSystem.loadingScreen(value);
    }

    public void ResumeGame() {
        GameAudioManager.Instance.PlayClickButton();
        _menuPanel.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _graphicRaycaster.enabled = false;
    }

    public void OpenMenu() {
        if(_gameOverPanel.activeSelf) return;
        GameAudioManager.Instance.PlayClickButton();
        _menuPanel.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _graphicRaycaster.enabled = true;
    }

    public void ExitGame() {
        GameAudioManager.Instance.PlayClickButton();
        _graphicRaycaster.enabled = false;
        Time.timeScale = 1;
        _loadingScreenBarSystem.loadingScreen(0);
    }
}
