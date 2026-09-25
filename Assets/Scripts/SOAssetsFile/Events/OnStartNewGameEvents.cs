using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / OnStartNewGameEvents")]
public class OnStartNewGameEvents : GameEvent<int> {
    [SerializeField] private string eventName = "OnStartNewGameEvents";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to start a new game";
    public string EventDescription => eventDescription;

    public void StartNewGame(int level)
    {
        Raise(level);
    }
}