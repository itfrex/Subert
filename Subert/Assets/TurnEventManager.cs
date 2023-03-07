using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnEventManager : MonoBehaviour
{
    public static TurnEventManager current;
    public event Action TurnEvent;

    private void Awake()
    {
        if (current == null)
            current = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    public void Turn()
    {
        TurnEvent?.Invoke();
    }
}
