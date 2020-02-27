using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechInterface : InitGO,IHelpTechology
{
    public TechButton techButton;
    public Text descr;
    public Tech Technology { get; set; }
    public void Init(Tech tech)
    {
        if (Technology != null)
            Technology.buttonEvent -= UpdateText;
        Technology = tech;
        tech.buttonEvent += UpdateText;
        techButton.SetTech(tech);
        descr.text = tech.ToString();
    }
    void UpdateText()
    {
        descr.text = Technology.ToString();
    }
    public override void Init(object initElement)
    {
        if (initElement is Tech tech)
            Init(tech);
    }
}
