using UnityEngine;
using System;
using UnityEngine.Events;


public class DualParamGameEvent<T1, T2> : ScriptableObject {
    public event Action<T1, T2> OnChange;

    public void Raise(T1 value1, T2 value2) {
        OnChange?.Invoke(value1, value2);
    }
}
