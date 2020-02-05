using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechInterface : InitGO
{
    public TechButton techButton;
    public Text descr;
    Tech tech;
    public void Init(Tech tech)
    {
        if (this.tech != null)
            this.tech.buttonEvent -= UpdateText;
        this.tech = tech;
        tech.buttonEvent += UpdateText;
        techButton.SetTech(tech);
        descr.text = tech.ToString();
    }
    void UpdateText()
    {
        descr.text = tech.ToString();
    }
    public override void Init(object initElement)
    {
        if (initElement is Tech tech)
            Init(tech);
    }
}
