using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyPanel : MonoBehaviour
{
    static Army curArmy;
    static ArmyPanel instance;
    public ListFiller army;
    public Image exchangeImage;
    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
        exchangeImage.gameObject.SetActive(false);
    }
    public static void Show(bool show)
    {
        curArmy = Player.army;
        show &= curArmy != null;
        instance.gameObject.SetActive(show);
        MenuManager.CheckExchangeRegiment();
        if (show)
            instance.Show();
    }
    public void Show()
    {
        UpdateArmy();
    }
    public void UpdateArmy()
    {
        army.UpdateList(curArmy.army.ConvertAll(x => (object)x));

    }
}
