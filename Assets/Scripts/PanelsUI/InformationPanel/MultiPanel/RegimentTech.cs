using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegimentTech : MonoBehaviour
{
    public TechButton techButton;
    public Image level;
    public Text timeCost, scienceCost;
    public Technology Technology { get; set; }
    public void Init(Technology tech)
    {
        if (Technology != null)
            Technology.buttonEvent -= UpdateText;
        Technology = tech;
        timeCost.gameObject.SetActive(true);
        scienceCost.gameObject.SetActive(true);
        tech.buttonEvent += UpdateText;
        techButton.SetTech(tech);
        UpdateText();
    }
    void UpdateText()
    {
        level.sprite = SpriteHandler.GetPipsSprite(Technology.lvl);
        timeCost.text = Technology.curTime.ToString("N1");
        scienceCost.text = Technology.curScience.ToString("N1");
        if (Technology.lvl == Technology.lvlmax)
        {
            timeCost.gameObject.SetActive(false);
            scienceCost.gameObject.SetActive(false);
        }
    }
}
