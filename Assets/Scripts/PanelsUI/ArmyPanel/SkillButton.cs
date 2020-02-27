using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private Button Skill;
    [SerializeField] private Button LevelUp;
    [SerializeField] private Text Level;

    private Skill CurrentSkill;
    public delegate bool Upgrade(Skill skill);
    public Upgrade upgrade;
    public void RemoveAllListeners()
    {
        Skill.onClick.RemoveAllListeners();
        LevelUp.onClick.RemoveAllListeners();
    }
    public void AddListener(UnityEngine.Events.UnityAction call)
    {
        Skill.onClick.AddListener(call);
    }
    public void AddUpgradeListener(UnityEngine.Events.UnityAction call)
    {
        LevelUp.onClick.AddListener(call);
    }
    public void SetSkill(Skill skill, Upgrade upgrade)
    {
        this.upgrade = upgrade;
        CurrentSkill = skill;
        gameObject.SetActive(true);
        Skill.image.sprite = SpriteHandler.GetSkillSprite(skill.type);
        Skill.onClick.AddListener(() => UpdateState());
        LevelUp.onClick.AddListener(() => UpdateState());
        UpdateState();
    }
    void Start()
    {
        
    }
    private float r, g, b, a;
    private Color state;
    public void UpdateState()
    {
        Level.text = CurrentSkill.GetLevel.ToString();
        if(CurrentSkill is SkeletonArmySkill sa)
        {
            Level.text += "(" + sa.KilledRegiment + ")";
        }
        r = 0;
        r += CurrentSkill.GetLevel > 0 ? 0.2f : 0;
        r += CurrentSkill.Active ? 0.02f : 0;
        g = CurrentSkill.SkillAction == null ? 1 : CurrentSkill.SkillAction.progress;
        
        var off = Skill.image.sprite.rect;
        b = (off.x)/ 1024;
        a = (off.y ) / 1024;
        state = new Color(r, g, b, a);
        
        if (CurrentSkill.SkillAction != null)
            CurrentSkill.SkillAction.onAction += UpdateState;
        Skill.image.color = state;
        Skill.interactable = (CurrentSkill.GetLevel != 0 && CurrentSkill.CanUse);
        UpdateLevelUp();
    }
    public void UpdateLevelUp()
    {
        LevelUp.gameObject.SetActive(upgrade(CurrentSkill));
    }
    void Update()
    {
        if(CurrentSkill?.SkillAction !=null)
        {
            g = CurrentSkill.SkillAction.progress;
            state.g = g;
            Skill.image.color = state;
        } 
        else
        {
            state.g = 1;
            Skill.image.color = state;
        }
    }
}
