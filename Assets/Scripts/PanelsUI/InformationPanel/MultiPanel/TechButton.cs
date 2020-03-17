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
    Tech tech;
    static Color blue = new Color(0.4f, 0.7f, 1f);
    void Start()
    {
        
    }
    public void StartResearch()
    {
        Player.curPlayer.SpendTreasure(new Treasury(0, 0, 0, 0, tech.science), BudgetType.TechnologyBudget);
        tech.researchAction = new ResearchAction(tech, tech.time);
        front.color = blue;
        button.interactable = false;
        descr.text = "0 %";
    }
    void Complete()
    {

        button.interactable = false;
        front.color = tech.ableResearch ? Color.gray : Color.green;
        front.fillAmount = 1f;
        descr.text = tech.ableResearch ? "блок." : "макс.";

    }
    public void EndResearch()
    {
        front.color = blue;
        if (!tech.able)
            Complete();
        notres = tech.researchAction == null;

        if (tech.able)
        {
            front.color = colorAvailable;
            button.interactable = true;
            Bar();
        }
    }
    public void SetTech(Tech technology)
    {
        if (tech != null)
            tech.buttonEvent -= EndResearch;
        tech = technology;
        if (!tech.able)
            Complete();

        if(tech.able)
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
            front.fillAmount = Mathf.Clamp01(Player.curPlayer.treasury.Science / tech.science);
            button.interactable = front.fillAmount >= 1f;
            descr.text = tech.science.ToString();
        }
        else
        {
            front.fillAmount = tech.researchAction.progress;
            int pc = (int)(front.fillAmount * 100);
            descr.text = pc + " %";
        }
    }
    bool notres;
    void Update()
    {
        if (!tech.able)
            return;
        if (tech.researchAction == null)
        {
            front.fillAmount = Mathf.Clamp01(Player.curPlayer.treasury.Science / tech.science);
            button.interactable = front.fillAmount >= 1f;
            descr.text = tech.science.ToString();
            if (!notres)
                EndResearch();
        }
        else
        {
            front.fillAmount = tech.researchAction.progress;
            int pc = (int)(front.fillAmount * 100);
            descr.text = pc + " %";
        }
        notres = tech.researchAction == null;
    }
}
