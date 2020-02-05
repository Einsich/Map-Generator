using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void ShowSmth(bool show);
public class ButtonSelector 
{

    ShowSmth[] shows;
    bool[] buttonsSelect;
    Button[] wievButtons;
    int ButtonCount;
    int current;
    public ButtonSelector(Button[] buttons, ShowSmth[] smths)
    {
        ButtonCount = buttons.Length;
        if (ButtonCount != smths.Length)
            throw new System.Exception("Beda beda");
        shows = smths;
        buttonsSelect = new bool[ButtonCount];
        wievButtons = buttons;
        for (int i = 0; i < ButtonCount; i++)
        {
            int k = i;
            wievButtons[i].onClick.AddListener(() => SetMenuMod(k));
            smths[i](false);
        }
        current = -1;
    }
    public void Update()
    {
        for (int i = 0; i < ButtonCount; i++)
            if (buttonsSelect[i])
                shows[i](true);
    }
    public void Update(int i)
    {
        if (buttonsSelect[i])
            shows[i](true);
    }
    public void Hidden(int k)
    {
        if (buttonsSelect[k])
        {
            SwapSprites(k, false);
            if (current == k)
                current = -1;
        }
    }
    public void SetMenuMod(int k)
    {
        bool a = buttonsSelect[k];

        if (current != -1)
        {
            SwapSprites(current, false);
            
        }
        if (current != k)
        {
            SwapSprites(k, true);
        }
        current = (current == k) ? -1 : k;
    }
    void SwapSprites(int i, bool show)
    {
        SpriteState state = wievButtons[i].spriteState;
        state.highlightedSprite = state.pressedSprite;
        state.pressedSprite = wievButtons[i].image.sprite;
        wievButtons[i].image.sprite = state.highlightedSprite;
        state.selectedSprite = state.pressedSprite;
        wievButtons[i].spriteState = state;
        buttonsSelect[i] = show;
        shows[i](show);
    }
}
