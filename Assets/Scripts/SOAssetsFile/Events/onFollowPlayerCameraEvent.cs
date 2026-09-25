using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / OnFollowPlayerCameraEvent")]
public class onFollowPlayerCameraEvent : GameEvent<GameObject> {
    [SerializeField] private string eventName = "onFollowPlayerCameraEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to follow player camera";
    public string EventDescription => eventDescription;

    public void FollowPlayerCamera(GameObject player)
    {
        Raise(player);
    }
}