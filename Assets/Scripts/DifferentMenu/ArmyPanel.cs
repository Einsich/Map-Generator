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
    public Image icon;
    public Image[] pips;
    public Image expFill;
    public Text exp;
    public Text name;
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
        Person person = curArmy.genegal;
        icon.sprite = person.icon;
        for (int i = 0; i < 3; i++)
            pips[i].sprite = ProvinceMenu.GetPips(person.pips[i]);
        expFill.fillAmount = person.expf;
        exp.text = $"{person.lvl} ур. {person.exp} / {person.nextLvl}";
        name.text = person.name;
    }
    public void UpdateArmy()
    {
        army.UpdateList(curArmy.army.ConvertAll(x => (object)x));

    }
}
