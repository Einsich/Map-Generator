using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonInformation : InitGO
{
    public Text name;
    public Image icon;
    public Image[] pips;
    public Text type;
    public Button toAlive, toCapital;
    public Image progress;
    public GameObject aliveProgress;
    public Person person;
    bool cantoCapital => !person.die && person.inTavern  && person.owner.Capital.ocptby == null && !person.owner.Capital.isBusy();
    public override void Init(object initElement)
    {
        person = (Person)initElement;
        name.text = person.name;
        icon.sprite = person.icon;
        icon.color = person.die ? new Color(0.1f,0.1f,0.1f) : Color.white;
        for (int i = 0; i < 3; i++)
            pips[i].sprite = SpriteHandler.GetPipsSprite(person.pips[i]);
        type.text = person.personType.ToString();
        toAlive.gameObject.SetActive(person.die && person.alive == null);
        toCapital.gameObject.SetActive(cantoCapital);
        aliveProgress.gameObject.SetActive(person.alive != null);
        wasdead = person.die;
    }
    public void DoAlive()
    {
        person.alive = new PersonAliveAction(person, GameConst.PersonAliveTime);
        wasdead = true;
        toAlive.gameObject.SetActive(false);
        aliveProgress.gameObject.SetActive(true);
    }
    public void DoToCapital()
    {
        toCapital.gameObject.SetActive(false);
        person.SetToCapital();
    }
    bool wasdead;
    void Update()
    {
        if (person.alive != null)
        {
            progress.fillAmount = person.alive.progress;
        }
        else if (wasdead)
        {
            wasdead = false;
            toCapital.gameObject.SetActive(cantoCapital);
            aliveProgress.gameObject.SetActive(false);
            icon.color = person.die ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
        }
    }
}
