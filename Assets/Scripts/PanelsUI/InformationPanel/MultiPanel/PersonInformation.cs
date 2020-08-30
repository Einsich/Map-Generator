using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonInformation : InitGO,IHelpPerson
{
    public Text name;
    public Image icon;
    public Text type;
    public Button toAlive, toCapital;
    public Image progress;
    public GameObject aliveProgress;
    public Person person;
    public Person Person { get => person; set { } }
    public override void Init(object initElement)
    {
        person = (Person)initElement;
        name.text = person.name;
        icon.sprite = person.icon;
        icon.color = person.die ? new Color(0.1f,0.1f,0.1f) : Color.white;


        type.text = person.personType.ToString();
        toAlive.gameObject.SetActive(person.needAlive);
        toCapital.gameObject.SetActive(person.curArmy != null && person.cantoCapital);
        aliveProgress.gameObject.SetActive(person.alive != null);
        wasdead = person.die;
    }
    public void DoAlive()
    {
        person.LivenUp();
        wasdead = true;
        toAlive.gameObject.SetActive(false);
        aliveProgress.gameObject.SetActive(true);
    }
    public void DoToCapital()
    {
        toCapital.gameObject.SetActive(false);
        person.SetToCapital();
        MainStatistic.Instance.buttonSelector.Update();
    }
    bool wasdead;
    void Update()
    {
        if (person.curArmy != null)
        {
            if (person.alive != null)
            {
                progress.fillAmount = person.alive.progress;
            }
            else if (wasdead)
            {
                wasdead = false;
                toCapital.gameObject.SetActive(person.cantoCapital);
                aliveProgress.gameObject.SetActive(false);
                icon.color = person.die ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
            }
        } else
        {
            if(person.owner.unlockPersons() < person.owner.technologyTree.maxPerson)
            {
                toCapital.gameObject.SetActive(person.cantoCapital);
                aliveProgress.gameObject.SetActive(false);
                icon.color = Color.white;
            }
        }
    }
}
