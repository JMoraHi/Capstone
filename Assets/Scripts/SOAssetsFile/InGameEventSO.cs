using UnityEngine;
using System;
using UnityEngine.Events;

public abstract class GameEvent<T> : ScriptableObject
{
    public Action<T> OnEventRaised;
    public UnityEvent<T> unityEvent;
    public void Raise(T value)
    {
        OnEventRaised?.Invoke(value);
        unityEvent?.Invoke(value);
    }

    
}



//======================================================================================




















