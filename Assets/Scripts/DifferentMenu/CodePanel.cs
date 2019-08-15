using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodePanel : MonoBehaviour {

    public InputField cdpanel;
    public static CodePanel that;
    public static List<State> state;
    void Awake()
    {
        that = this;
    }
    void Start()
    {
        cdpanel.onValueChanged.AddListener(delegate { ChangedValue(cdpanel); });
    }
    public void Active(bool w)
    {
        cdpanel.gameObject.SetActive(w);
        if(w)
        cdpanel.ActivateInputField();
    }
    void Error()
    {
        notchange = true;
        cdpanel.text += "Error!\n";
        notchange = false;
        cdpanel.MoveTextEnd(false);
    }
    bool notchange;
    int lastLength = 0;
    void ChangedValue(InputField inp)
    {
        if (notchange) return;
        if (inp.text.Length > lastLength)
        {
            char c = inp.text[inp.text.Length - 1];
            if(c=='ё')
            {
                notchange = true;
                cdpanel.text = cdpanel.text.Replace('ё','\n');
                notchange = false;
                cdpanel.MoveTextEnd(false);
            }
            if (c == '\n')
            {
                EndEdit(Buf());
            }
        }
        lastLength = inp.text.Length;
    }
    string Buf()
    {
        int i;
        for (i = cdpanel.text.Length - 2; i >= 0; i--)
            if (cdpanel.text[i] == '\n') break;
        string s = "";
        for (i++; i < cdpanel.text.Length - 1; i++)
            s += cdpanel.text[i];
        return s;
    }
    void EndEdit(string s)
    {
        string[] t = s.Split(' ');
        if(t.Length==2)
        {
            int j = -1;
            switch(t[0])
            {
                case "страна":
                    for (int i = 0; i < state.Count; i++)
                        if (state[i].name == t[1])
                        {
                            j = i;
                            break;
                        }
                    if (j < 0) Error();
                    else Player.SetState(state[j]);
                    break;
                case "захват":
                    if (int.TryParse(t[1], out j) && j <= Player.reg.Count&& !Player.reg[j].iswater)
                    {
                        Player.that.Occupated(Player.reg[j], Player.curPlayer);
                    }
                    else Error();
                    break;
                default:Error();break;
            }
        }
        else
        {
            Error();
        }
    }
    bool activepanel = false;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            CameraController.cheat = Date.cheat = activepanel = !activepanel;
            Active(activepanel);
        }

    }

}
