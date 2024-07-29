using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class SnapInput<T>
{
    [SerializeField]
    private bool Debugging = true;
    public InputAction Bindings = new InputAction();
    [Tooltip("0 == Disabled\n1 == Waiting\n2 == Started\n3 == Performed\n4 == Cancelled")]
    [SerializeField] private UnityEvent<T>[] actions = new UnityEvent<T>[5];

    public void Check()
    {
        //Check for the correct amount of actions slots.
        if(actions.Length != 5)
        {
            Debug.LogError("Not enough action slots in " + this + " snapInput. Plz fix boss");
            return;
        }
        if(Debugging) Debug.Log("Binding phase is: " + Bindings.phase);
        //Try to call the actions in the event.
        try
        {
            actions[(int)Bindings.phase].Invoke((T)Bindings.ReadValueAsObject());
        }
        catch (Exception e)
        {
            //Catch any errors - usually from no or empty event calls.
            if (Debugging) Debug.LogWarning(e);
        }
    }
}
