using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyBar : MonoBehaviour
{
    public Image flag, count;
    //public Text label;
    private CanvasRenderer Renderer;
    private MaterialPropertyBlock Block;
    Army army;
    RectTransform trans;
    private float Health = 1f, Hit = 1f;
    private void Awake()
    {
        trans = GetComponent<RectTransform>();
        transform.SetAsFirstSibling();
        Renderer = count.canvasRenderer;
        Block = new MaterialPropertyBlock();
        //count.
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
    float LastHit = 0;
    public void UpdateInformation()
    {
        float hp = army.MediumCount();
        if (hp > Health)
            Health = Hit = hp;
        else
        {
            Health = hp;
            LastHit = Time.time;
        }
    }    

    void Update()
    {
        trans.anchoredPosition = Camera.main.WorldToScreenPoint(army.transform.position);

        if (!Date.play)
            return;
        flag.color = LastHit + 0.2 < Time.time ? Color.white : Color.green;
        if (Hit > Health)
        {
            Hit -= 0.5f * Time.deltaTime;
        }
        count.color = new Color(Health, Hit, 2f / army.army.Count);
    }
}
