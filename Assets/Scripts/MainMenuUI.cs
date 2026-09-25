
using UnityEngine;

using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private OnStartNewGameEvents _onStartNewGameEvents;
    [SerializeField] private LoadingScreenBarSystem _loadingScreenBarSystem;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private GameObject _settingsPanel;

    private void OnEnable() {
        _onStartNewGameEvents.OnEventRaised += StartNewGame;
    }

    private void OnDisable() {
        _onStartNewGameEvents.OnEventRaised -= StartNewGame;
    }

    void StartNewGame(int level)
    {
        GameAudioManager.Instance.PlayClickButton();
        _loadingScreenBarSystem.loadingScreen(level);
        // SceneManager.LoadScene(level);
    }

    private void Awake() {
        _settingsPanel.SetActive(false);
        _settingsButton.onClick.AddListener(OpenSettings);
    }

    void OpenSettings(){
        GameAudioManager.Instance.PlayClickButton();
        _settingsPanel.SetActive(true);
    }

    public void CloseSettings(){
        GameAudioManager.Instance.PlayClickButton();
        _settingsPanel.SetActive(false);
    }


    public void ExitGame(){
        GameAudioManager.Instance.PlayClickButton();
        Application.Quit();
    }
}
