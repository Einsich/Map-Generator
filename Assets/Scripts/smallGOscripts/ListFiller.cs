using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListFiller : MonoBehaviour
{
    
    List<InitGO> list = new List<InitGO>();
    public InitGO prefab;
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
            element.Init(x);
        }
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, (size.y + 1) * i);
        for (; i < list.Count; i++)
            list[i].gameObject.SetActive(false);
    }

   
}
public interface IInit
{
    void Init(object initElement);
}
public abstract class InitGO : MonoBehaviour, IInit
{
    public abstract void Init(object initElement);
}
