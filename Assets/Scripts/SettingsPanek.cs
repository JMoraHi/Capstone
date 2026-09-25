
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanek : MonoBehaviour
{
    [Header("Audio SO")]
    [SerializeField] private AudioSettingsSO _VolumeSettings;
    [SerializeField] private PlayerSettingsSO _PlayerSettings;

    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Slider _senstivitySlider;

    private void Awake() {
        _bgmVolumeSlider.value = _VolumeSettings.bgmVolume;
        _sfxVolumeSlider.value = _VolumeSettings.sfxVolume;
        _senstivitySlider.value = _PlayerSettings.mouseCamRotationSpeed;
    }

    public void SetBgmVolume(){
        _VolumeSettings.bgmVolume = _bgmVolumeSlider.value;
        GameAudioManager.Instance._bgmSource.volume = _VolumeSettings.bgmVolume;
    }

    public void SetSfxVolume(){
        _VolumeSettings.sfxVolume = _sfxVolumeSlider.value;
        GameAudioManager.Instance._sfxSource.volume = _VolumeSettings.sfxVolume;
    }

    public void SetSenstivity(){
        _PlayerSettings.mouseCamRotationSpeed =_senstivitySlider.value ;
    }

}
