using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyPanel : MonoBehaviour
{
    [SerializeField] TechnologyTreeUI treeUI;
    [SerializeField] Button left, right;
    int curPage = 0;
    private State treeOwner = null;
    private void Awake()
    {
        left.onClick.AddListener(() => Switch(-1));
        right.onClick.AddListener(() => Switch(1));
    }
    public void ShowTechnology(TechnologyTree technology)
    {
        if (technology.state != Player.curPlayer)
            return;
        if(technology.state != treeOwner)
        {
            treeOwner = technology.state;
            treeUI.LoadData(technology);
        }
        Switch(0);
    }
    private void Switch(int d)
    {
        curPage = Mathf.Clamp(curPage + d, 0, 2);

        if (d > 0)
            left.interactable = true;
        if (d < 0)
            right.interactable = true;

        if (curPage == 0)
            left.interactable = false;
        if (curPage == 2)
            right.interactable = false;
        treeUI.ShowPage(curPage);
    }
}
