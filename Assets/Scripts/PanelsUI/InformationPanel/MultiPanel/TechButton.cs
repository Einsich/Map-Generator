using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechButton : MonoBehaviour
{
    [SerializeField] public Color colorAvailable;
    public Image back, front;
    public Button button;
    public Text descr;
    Technology tech;
    static Color blue = new Color(0.4f, 0.7f, 1f);
    bool newRegimnetTech = false;
    public void StartResearch()
    {
        Player.curPlayer.SpendTreasure(new Treasury(0, 0, 0, 0, tech.curScience));
        tech.researchAction = new ResearchAction(tech, tech.curTime);
        front.color = blue;
        button.interactable = false;
        descr.text = "0 %";
    }
    void Complete()
    {

        button.interactable = false;
        front.color = tech.notLimited ? Color.gray : Color.green;
        front.fillAmount = 1f;
        descr.text = tech.notLimited ? "блок." : "макс.";

    }
    public void EndResearch()
    {
        front.color = blue;
        if (!tech.researchOrAbleResearch)
            Complete();
        notres = tech.researchAction == null;

        if (tech.researchOrAbleResearch)
        {
            front.color = colorAvailable;
            button.interactable = true;
            Bar();
        }
    }
    public void SetTech(Technology technology)
    {
        newRegimnetTech = technology is NewRegimentTechnology;
        if (tech != null)
            tech.buttonEvent -= EndResearch;
        tech = technology;
        if (!tech.researchOrAbleResearch)
            Complete();

        if(tech.researchOrAbleResearch)
        {
            button.interactable = notres = tech.researchAction == null;
            front.color = notres ? colorAvailable : blue;            
            Bar();
        }
        tech.buttonEvent += EndResearch;
    }
    void Bar()
    {
        if (notres)
        {
            front.fillAmount = Mathf.Clamp01(TechnologyTreeUI.treeOwner.treasury.Science / tech.curScience);
            button.interactable = front.fillAmount >= 1f;
            descr.text = "";
        }
        else
        {
            front.fillAmount = tech.researchAction.progress;
            front.color = blue;
            int pc = (int)(front.fillAmount * 100);
            descr.text = pc + " %";
        }
    }
    bool notres = false;
    void Update()
    {
        if (tech == null ||!tech.researchOrAbleResearch)
            return;
        if (tech.researchAction == null)
        {
            front.fillAmount = Mathf.Clamp01(TechnologyTreeUI.treeOwner.treasury.Science / tech.curScience);
            button.interactable = front.fillAmount >= 1f;
            descr.text = tech.curScience.ToString("N1");
            front.color = colorAvailable;
            if (!notres)
                EndResearch();
        }
        else
        {
            front.fillAmount = tech.researchAction.progress;
            front.color = blue;
            int pc = (int)(front.fillAmount * 100);
            descr.text = pc + " %";
        }
        notres = tech.researchAction == null;
    }
}
