using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegimentTeches : MonoBehaviour,IHelpBaseRegiment
{
    public RegimentTech[] Teches;//4
    public TechInterface researchRegiment;//1
    public Text Name;
    public Image Icon;

    public BaseRegiment BaseRegiment { get ; set ; }

    public void Init(BaseRegiment regiment, Technology[] regTeches, Technology researchedRegiment)
    {
        BaseRegiment = regiment;
        Icon.sprite = regiment.Icon;
        Name.text = regiment.nameRegiment;
        for (int i = 0; i < regTeches.Length; i++)
        {
            Teches[i].Init(regTeches[i]);       
        }
        if (researchedRegiment != null)
            researchRegiment.Init(researchedRegiment);
        researchRegiment.gameObject.SetActive(researchedRegiment != null);
    }
}
