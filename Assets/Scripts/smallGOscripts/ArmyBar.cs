using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyBar : MonoBehaviour
{
    public Image flag, count, moral;
    public Text label;
    Army army;
    RectTransform trans;
    private void Awake()
    {
        trans = GetComponent<RectTransform>();
        transform.SetAsFirstSibling();
    }
    public Army currentArmy { set
        {
            if (army != null)
                army.bar = null;
            army = value;
            army.bar = this;
            flag.sprite = army.owner.flagSprite;
            UpdateInformation();
        }
    }
    public void UpdateInformation()
    {
        float medcount = army.MediumCount();
        float medmoral = army.MediumMoral();
        count.fillAmount = medcount;
        moral.fillAmount = medmoral;
        label.text = (medcount * army.army.Count).ToString("n1") + " k";
    }

    void Update()
    {
        trans.anchoredPosition = Camera.main.WorldToScreenPoint(army.transform.position);
    }
}
