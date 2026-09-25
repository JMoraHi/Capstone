
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayGameButtonUI : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] OnLevelChangeEvents _onLevelChangeEvents;
    
    [Header("References")]
    [SerializeField] private GameObject playGameButton;
    [SerializeField] TMP_Dropdown difficultyDropdown;



    public void onChangeLevel()
    {
        GameAudioManager.Instance.PlayClickButton();
        _onLevelChangeEvents.Raise(difficultyDropdown.value);
    }

    private void Awake()
    {
        playGameButton.GetComponent<Button>().onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        GameManager.Instance.StartNewGame();        
    }

}
