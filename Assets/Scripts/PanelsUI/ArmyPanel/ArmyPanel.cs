﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyPanel : MonoBehaviour, IHelpPerson
{
    static Army curArmy;
    public static ArmyPanel Instance;
    public ListFiller army;
    public Image exchangeImage;
    public Image icon;
    public SkillButton[] skillsButton;
   // public Button[] skillsUpButton;
    public Image expFill;
    public Text exp;
    public Text name, regimentsCount;
    public Person Person { get; set; }
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
        exchangeImage.gameObject.SetActive(false);
    }
    public static void Show(bool show)
    {
        curArmy = Player.army == null? curArmy : Player.army;
       
        show &= curArmy != null;
        Instance.gameObject.SetActive(show);

        if (curArmy)
        {
            Instance.Person = curArmy.Person;
            MenuManager.CheckExchangeRegiment();
            if (show)
            {
                Instance.Show();
                curArmy.HitAction += Instance.UpdateArmy;
                curArmy.ArmyListChange += Instance.UpdateArmy;
                curArmy.Person.ExpChanged += Instance.UpdatePropertyAndSkills;
                curArmy.Person.SkillCulldowned += Instance.UpdatePropertyAndSkills;
            }
            else
            {
                curArmy.HitAction -= Instance.UpdateArmy;
                curArmy.ArmyListChange -= Instance.UpdateArmy;
                curArmy.Person.ExpChanged -= Instance.UpdatePropertyAndSkills;
                curArmy.Person.SkillCulldowned -= Instance.UpdatePropertyAndSkills;
            }
            for (int i = 0; i < curArmy.Person.Skills.Length; i++)
                if (show)
                    curArmy.Person.Skills[i].ButtonUpdate += Instance.skillsButton[i].UpdateState;
                else
                    curArmy.Person.Skills[i].ButtonUpdate -= Instance.skillsButton[i].UpdateState;

        }

    }
    public void Show()
    {
        UpdateArmy();
        Person person = Person;
        icon.sprite = person.icon;
        int i;
        for(i=0;i<person.Skills.Length;i++)
        {
            Skill skill = person.Skills[i];
            SkillButton button = skillsButton[i];
            button.gameObject.SetActive(true);
            button.RemoveAllListeners();
            button.AddListener(() => skill.Cast());
            button.AddListener(() => UpdatePropertyAndSkills());
            int j = i;
            button.AddUpgradeListener(() => person.LearnSkill(j));
            button.AddUpgradeListener(() => UpdatePropertyAndSkills());
            button.SetSkill(skill, (s) => s.Person.lvlPoint > 0 && s.GetLevel != s.GetMaxLevel);
            
        }
        for (; i < skillsButton.Length; i++)
            skillsButton[i].gameObject.SetActive(false);
        name.text = person.name;
        UpdatePropertyAndSkills();
    }
    public void UpdatePropertyAndSkills()
    {

        Person person = curArmy.Person;
        expFill.fillAmount = person.expf;
#if DEVELOP
        exp.text = curArmy.AI.curBehavior;
#else
        exp.text =  $"{person.lvl} ур. {(int)person.exp} / {person.nextLvl}";
#endif
        
        for (int i = 0; i < person.Skills.Length; i++)
        {
            skillsButton[i].UpdateLevelUp();
        }



    }
    public void UpdateArmy()
    {
        regimentsCount.text = $"{curArmy.army.Count}/{Person.MaxRegiment}";
        army.UpdateList(curArmy.army.ConvertAll(x => (object)x));

    }
    public Skill GetSkill(int k)
    {
        return curArmy?.Person.Skills.Length > k ? curArmy.Person.Skills[k] : null;
    }
}
