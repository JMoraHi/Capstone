
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] public AudioSource _bgmSource;
    [SerializeField] public AudioSource _sfxSource;


    [Header("Audio Clips")]
    [SerializeField] private AudioClip _bgmClip;
    [SerializeField] private AudioClip _inGameBgmClip;
    [SerializeField] private AudioClip _clickClip;
    [SerializeField] private AudioClip _loadingClip;
    [SerializeField] private AudioClip _gameOverClip;
    [SerializeField] private AudioClip _gameWinClip;

    [Header("Volume")]
    [SerializeField] private AudioSettingsSO _VolumeSettings; 


    private void Awake()
    {
        _bgmSource.volume = _VolumeSettings.bgmVolume;
        _sfxSource.volume = _VolumeSettings.sfxVolume;
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject);}
            else{Destroy(gameObject);}
    }

    public void PlayBgm(){
        StopAudio();
        _bgmSource.clip = _bgmClip;
        _bgmSource.volume = _VolumeSettings.bgmVolume;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }


    public void PlayClickButton(){
        _sfxSource.volume = _VolumeSettings.sfxVolume;
        _sfxSource.PlayOneShot(_clickClip);
    }

    public void PlayeLoadingBgm(){
        StopAudio();
        _bgmSource.clip = _loadingClip;
        _bgmSource.volume = _VolumeSettings.bgmVolume;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }


    public void PlayInGameSoundBgm(){
        StopAudio();
        _bgmSource.clip = _inGameBgmClip;
        _bgmSource.volume = _VolumeSettings.bgmVolume;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void StopAudio(){
        _bgmSource.Stop();
        _sfxSource.Stop();
    }

    public void PlayLose(){
        StopAudio();
        _bgmSource.clip = _gameOverClip;
        _bgmSource.volume = _VolumeSettings.bgmVolume;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }
    public void PlayWin(){
        StopAudio();
        _sfxSource.PlayOneShot(_gameWinClip);
    }


}
