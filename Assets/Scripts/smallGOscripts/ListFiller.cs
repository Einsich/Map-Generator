using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListFiller : MonoBehaviour
{
    
    List<InitGO> list = new List<InitGO>();
    public InitGO prefab;
    public int column = 1;
    RectTransform rect;
    Vector2 size;
    private void Initialize()
    {
        size = prefab.GetComponent<RectTransform>().sizeDelta;
        rect = GetComponent<RectTransform>();
    }

    public void UpdateList(List<object>listEl)
    {
        if (rect == null)
            Initialize();
        int i = 0;
        InitGO element;
        foreach (var x in listEl)
        {
            if (i < list.Count)
                element = list[i];
            else
                list.Add(element = Instantiate(prefab, transform));
            i++;
            element.gameObject.SetActive(false);
            element.gameObject.SetActive(true);
            element.Init(x);
        }
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, (size.y ) * ((i + column - 1) / column));
        for (; i < list.Count; i++)
            list[i].gameObject.SetActive(false);
    }

   
}

public abstract class InitGO : MonoBehaviour
{
    public abstract void Init(object initElement);
}
