using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New InGameEvent", menuName = "New Event / OnAmmoChangeEvent")]
public class OnAmmoChangeEvent : DualParamGameEvent<int, int> { 
    [SerializeField] private string eventName = "OnAmmoChangeEvent";
    public string EventName => eventName;
    [SerializeField] private string eventDescription = "Event to change player ammos";
    public string EventDescription => eventDescription;

    public void ChangePlayerAmmosUI(int currentAmmos, int maxAmmos)
    {
        Raise(currentAmmos, maxAmmos);
    }
}