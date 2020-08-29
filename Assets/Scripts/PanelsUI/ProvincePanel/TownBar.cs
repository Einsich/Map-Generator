using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class TownBar : MonoBehaviour, IPointerClickHandler
{
    public Image flag, count;
    private CanvasRenderer Renderer;
    private MaterialPropertyBlock Block;
    Region region;
    RectTransform trans;
    private float Health = 1f, Hit = 1f, Count = 1f;
    private void Awake()
    {
        trans = GetComponent<RectTransform>();
        transform.SetAsFirstSibling();
        Renderer = count.canvasRenderer;
        Block = new MaterialPropertyBlock();
    }

    public bool Active
    {
        set
        {
            value &= MapMetrics.Visionable(region.Capital);
            if(gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);
                if (value)
                    UpdateInformation();
            }
        }
    }
    public Region currentRegion
    {
        set
        {
            if (region != null)
                region.bar = null;
            region = value;
            Active = false;
        }
    }
    float LastHit = 0;
    public void UpdateInformation()
    {
        flag.sprite = region.owner.flagSprite;
        float hp = region.curHP;
        if (hp >= Health)
            Health = Hit = hp;
        else
        {
            Health = hp;
            LastHit = Time.time;
        }
        Count = region.data.garnison.Count == 0 ? 0 : 2f / region.data.garnison.Count;
        count.color = new Color(Health, Hit, Count);
        trans.anchoredPosition = Camera.main.WorldToScreenPoint(region.worldPosition);
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;
        trans.anchoredPosition = Camera.main.WorldToScreenPoint(region.worldPosition);

        if (!Date.play)
            return;
        flag.color = LastHit + 0.2 < Time.time ? Color.white : Color.green;
        if (Hit > Health)
        {
            Hit -= 0.5f * Time.deltaTime;
        }
        count.color = new Color(Health, Hit, Count);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Player.RegionTap(region);
    }
}
