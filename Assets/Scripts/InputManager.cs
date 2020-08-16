
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class InputManager : MonoBehaviour
{

    private Dictionary<KeyCode, Action> actions = new Dictionary<KeyCode, Action>();

    public Action this[KeyCode code]
    {
        get { return actions.ContainsKey(code) ? actions[code] : delegate { } ; }
        set { actions[code] = value; }
    }

    public static InputManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (!Input.anyKey)
        {
            return;
        }
        for (KeyCode code = KeyCode.None; code < KeyCode.End; code++)
        {
            if (Input.GetKeyDown(code) && actions.ContainsKey(code))
                    actions[code]?.Invoke();
        }
    }

}