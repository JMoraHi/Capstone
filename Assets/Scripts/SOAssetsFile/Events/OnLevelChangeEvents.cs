using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / OnLevelChangeEvent")]
public class OnLevelChangeEvents : GameEvent<int> {
    [SerializeField] private string eventName = "OnLevelChangeEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to change the level";
    public string EventDescription => eventDescription;

    public void ChangeLevel(int level)
    {
        Raise(level);
    }
}
