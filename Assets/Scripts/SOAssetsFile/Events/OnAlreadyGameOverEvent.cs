using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / onAlreadyGameOverEvent")]
public class OnAlreadyGameOverEvent : GameEvent<bool> {
    [SerializeField] private string eventName = "OnAlreadyGameOverEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to change game over";
    public string EventDescription => eventDescription;

    public void GameOver(bool isGameOver)
    {
        Raise(isGameOver);
    }
}