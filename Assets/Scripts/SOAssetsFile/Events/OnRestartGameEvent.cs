using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / OnRestartGameEvent")]
public class OnRestartGameEvent : GameEvent<int> {
    [SerializeField] private string eventName = "OnRestartGameEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to restart the game on Index";
    public string EventDescription => eventDescription;

    public void RestartGame(int buildIndex)
    {
        Raise(buildIndex);
    }
}
