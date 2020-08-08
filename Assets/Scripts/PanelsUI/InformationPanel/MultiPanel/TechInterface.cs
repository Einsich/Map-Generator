using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechInterface : InitGO,IHelpTechology
{
    public TechButton techButton;
    public Image icon;
    public Text nameTech, timeCost, scienceCost;
    public Technology Technology { get; set; }
    private void Awake()
    {
        if(icon != null)
        {
            var listener = icon.gameObject.AddComponent<HelpListener>();
            listener.direction = HelpPanelDirection.DownRight;
            listener.size = HelpPanelSize.Medium200x200;
            listener.advance = HelpShowClass.Technology;
        }
    }
    public void Init(Technology tech)
    {
        if (Technology != null)
            Technology.buttonEvent -= UpdateText;
        Technology = tech;
        tech.buttonEvent += UpdateText;
        techButton.SetTech(tech);
        if (icon != null)
            icon.sprite = tech.Icon;
        timeCost.gameObject.SetActive(true);
        scienceCost.gameObject.SetActive(true);
        UpdateText();
    }
    void UpdateText()
    {
        if (nameTech != null)
            nameTech.text = Technology.name;
        timeCost.text = Technology.curTime.ToString("N1");
        scienceCost.text = Technology.curScience.ToString("N1");
        if(Technology.lvl == Technology.lvlmax)
        {
            timeCost.gameObject.SetActive(false);
            scienceCost.gameObject.SetActive(false);
        }
    }
    public override void Init(object initElement)
    {
        if (initElement is Technology tech)
            Init(tech);
    }
}
