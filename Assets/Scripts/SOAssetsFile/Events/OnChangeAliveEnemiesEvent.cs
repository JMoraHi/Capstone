using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / OnChangeAliveEnemies")]
public class OnChangeAliveEnemiesEvent : DualParamGameEvent<int, int> { 
    [SerializeField] private string eventName = "OnChangeAliveEnemiesEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to change alive enemies";
    public string EventDescription => eventDescription;

    public void ChangeAliveEnemies(int totalAliveEnemies, int currentAliveEnemies)
    {
        Raise(totalAliveEnemies, currentAliveEnemies);
    }
}
