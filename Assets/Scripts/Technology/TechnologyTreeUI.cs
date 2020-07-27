using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyTreeUI : MonoBehaviour
{
    private TechnologyTree tree;
    public static State treeOwner;
    public Transform[] pages;
    private (RegimentTeches, TechInterface)[] technologypanels;
    private void Awake()
    {
        technologypanels = new (RegimentTeches, TechInterface)[1 + 1 + pages[0].childCount + pages[1].childCount + pages[2].childCount];
        CollectChilds(transform);
        CollectChilds(pages[0]);
        CollectChilds(pages[1]);
        CollectChilds(pages[2]);
    }
    private void CollectChilds(Transform parent)
    {
        foreach(Transform tr in parent)
        {
            int ind;
            if(int .TryParse(tr.name, out ind))
            {
                technologypanels[ind] = (tr.GetComponent<RegimentTeches>(), tr.GetComponent<TechInterface>());
            }
        }
    }
    public void ShowPage(int page)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].gameObject.SetActive(i == page);

    }
    public void LoadData(TechnologyTree tree)
    {
        treeOwner = tree.state;
        for (int i = 0; i < tree.technology.Count; i++)
        {

            Technology tech = tree.technology[i];
            int nodeId = tech.NodeID; 
            Debug.Log(nodeId);
            if (nodeId < 0)
                continue;
            (RegimentTeches regimentTeches, TechInterface techInterface) = technologypanels[nodeId];

            if (regimentTeches)
            {
                NewRegimentTechnology regimentTechnology = tech as NewRegimentTechnology;
                BaseRegiment baseRegiment = null;
                if (regimentTechnology != null)
                {
                    i++;
                    baseRegiment = regimentTechnology.regimentInstance;
                }
                Technology[] regTech = new Technology[4] { tree.technology[i], tree.technology[i + 1], tree.technology[i + 2], tree.technology[i + 3] };
                i += 3;
                if (baseRegiment == null)
                    baseRegiment = tree.regiments[(regTech[0] as AttackTechnology).regimentOffset];
                regimentTeches.Init(baseRegiment, regTech, regimentTechnology);
            }
            else
            {
                if (techInterface)
                {
                    techInterface.Init(tech);
                }
            }
        }
    }
}
