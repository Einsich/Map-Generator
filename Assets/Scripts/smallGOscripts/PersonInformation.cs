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
    public Button toAlive;
    public Image progress;
    public GameObject aliveProgress;
    public Person person;
    public override void Init(object initElement)
    {
        person = (Person)initElement;
        name.text = person.name;
        icon.sprite = person.icon;
        icon.color = person.die ? new Color(0.1f,0.1f,0.1f) : Color.white;
        for (int i = 0; i < 3; i++)
            pips[i].sprite = ProvinceMenu.GetPips(person.pips[i]);
        type.text = person.personType.ToString();
        toAlive.gameObject.SetActive(person.die && person.alive == null);
        aliveProgress.gameObject.SetActive(person.alive != null);
        wasdead = person.die;
    }
    public void DoAlive()
    {
        person.alive = new PersonAliveAction(person, GameConst.PersonAliveTime);
        toAlive.gameObject.SetActive(false);
        aliveProgress.gameObject.SetActive(true);
    }
    bool wasdead;
    void Update()
    {
        if (person.alive != null)
            progress.fillAmount = person.alive.progress;
        else if(wasdead)
        {
            wasdead = false;
            aliveProgress.gameObject.SetActive(false);
        }
    }
}
