using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ButtonInterface : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler {
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();
        if (image == null)
            image = GetComponent<Image>();
        inside = true;
        t = Time.time;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inside = false;
        if (sprite != null) sprite.color = Color.white;
        else image.color = Color.white;
    }
    void Start()
    {
        
    }
    public SpriteRenderer sprite;
    public Image image;
    bool inside=false;
    float t;

	void Update () {
        if (inside)
        {
            Color c = Color.Lerp(Color.gray, Color.white, Sqr(Mathf.Cos((Time.time - t) * 4)));
            if (sprite != null) sprite.color = c;
            else image.color = c;
        }
	}
    float Sqr(float x)
    {
        return x * x;
    }
}
