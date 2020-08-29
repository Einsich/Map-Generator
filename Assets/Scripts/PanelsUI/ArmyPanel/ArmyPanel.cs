using System.Collections;
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
    public Image[] pips;
    public Button[] pipsUpButton;
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
        for ( i = 0; i < pipsUpButton.Length; i++)
        {
            Button button = pipsUpButton[i];
            button.onClick.RemoveAllListeners();
            PropertyType j = (PropertyType)i;
            button.onClick.AddListener(() => person.LearnProperty(j));
            button.onClick.AddListener(() => UpdatePropertyAndSkills());
        }
        name.text = person.name;
        UpdatePropertyAndSkills();
    }
    public void UpdatePropertyAndSkills()
    {

        Person person = curArmy.Person;
        expFill.fillAmount = person.expf;
        exp.text = curArmy.AI.curBehavior;// $"{person.lvl} ур. {(int)person.exp} / {person.nextLvl}";

        pips[0].sprite = SpriteHandler.GetPipsSprite(person.LeadershipLvl);
        pips[1].sprite = SpriteHandler.GetPipsSprite(person.AttackSpeedLvl);
        pips[2].sprite = SpriteHandler.GetPipsSprite(person.MeleeBuffLvl);
        pips[3].sprite = SpriteHandler.GetPipsSprite(person.RangeBuffLvl);
        
        int i = 0;
        for (; i < pipsUpButton.Length; i++)
            pipsUpButton[i].gameObject.SetActive(person.lvlPoint > 0);
        for (i = 0; i < person.Skills.Length; i++)
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
