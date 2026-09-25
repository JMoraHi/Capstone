
using UnityEngine;

public class MainCameraControler : MonoBehaviour
{
    [SerializeField] private onFollowPlayerCameraEvent _onFollowPlayerCameraEvent;
    [SerializeField] private Cinemachine.CinemachineFreeLook _freeLookCamera;

    private void OnDisable() {
        _onFollowPlayerCameraEvent.OnEventRaised -= FollowPlayer;
    }

    private void OnEnable() {
        _onFollowPlayerCameraEvent.OnEventRaised += FollowPlayer;
    }


    private void FollowPlayer(GameObject player) {
        _freeLookCamera.Follow = player.transform;
        _freeLookCamera.LookAt = player.transform;
    }
}
