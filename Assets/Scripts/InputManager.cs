
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class InputManager : MonoBehaviour
{
    const int EventCount = 8;

    [Header("Esc", order = 0), Header("Space", order = 1), Header("Enter", order = 2), Space(3),Header("S Q W E",order =3)]
    //значения обрабатываемых клавиш и сами события
    [SerializeField] private KeyCode[] KeyCodes = new KeyCode[EventCount];
    [SerializeField] private System.Action[] Events = new System.Action[EventCount];

    public delegate void PressEvent(KeyCode code);
    public event PressEvent KeyPressed;

    //интерфейс для подписки/ отписки
    public System.Action EscAction { get => Events[0]; set => Events[0] = value; }
    public System.Action SpaceAction { get => Events[1]; set => Events[1] = value; }
    public System.Action EnterAction { get => Events[2]; set => Events[2] = value; }
    public System.Action S { get => Events[3]; set => Events[3] = value; }
    public System.Action Q { get => Events[4]; set => Events[4] = value; }
    public System.Action W { get => Events[5]; set => Events[5] = value; }
    public System.Action E { get => Events[6]; set => Events[6] = value; }
    public System.Action F12 { get => Events[7]; set => Events[7] = value; }

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

        for (int i = 0; i < EventCount; i++)
        {
            if (Input.GetKeyDown(KeyCodes[i]))
            {
                Events[i]?.Invoke();
            }
        }
        
        if (KeyPressed != null)
        {
            for (KeyCode code = (KeyCode)0; code < KeyCode.End; code++)
            {
                KeyPressed(code);
            }
        }
    }
    public void SetAction(System.Action action, KeyCode code)
    {
        for (int i = 0; i < EventCount; i++)
        {
            if (action == Events[i])
            {
                KeyCodes[i] = code;
            }
        }
    }

}