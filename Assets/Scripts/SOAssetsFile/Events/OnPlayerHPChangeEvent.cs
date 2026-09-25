using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / OnHealthChangeEvent")]
public class OnPlayerHPChangeEvent : DualParamGameEvent<float, float> { 
    [SerializeField] private string eventName = "OnPlayerChangeEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to change player health";
    public string EventDescription => eventDescription;

    public  void ChangePlayerHealth(float currentHP, float maxHP)
    {
        Raise(currentHP, maxHP);
    }
}
