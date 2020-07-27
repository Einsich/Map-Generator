using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class TechnologyNode : MonoBehaviour
{
    public int NodeID = -1;
    public Technology technology;
    TechInterface techInterface;
    [HideInInspector] public Vector2 treePosition;

    void Update()
    {
        if (technology != null && (TechnologyTreeBuilder.build))
        {
            
            if (TechnologyTreeBuilder.PanelTransform != null)
            {
                if (techInterface == null)
                {
                    techInterface = Instantiate(PrefabHandler.TechnologyPanelPrefab, TechnologyTreeBuilder.PanelTransform);
                }
                techInterface.transform.SetAsLastSibling();
                NodeID = techInterface.transform.parent.childCount - 1;
                techInterface.name = NodeID.ToString();
                techInterface.Init(technology);
                techInterface.GetComponent<RectTransform>().anchoredPosition = treePosition;
            }
            name =  technology.name;
            int maxChild = MaxChild();
            float childFactor = (float)maxChild / transform.childCount;
            Vector2 offset = new Vector2(-200 * maxChild * 0.5f, -160);
            Vector2 delta = new Vector2(200* childFactor, 0);
            int i = 0;
            foreach (Transform trans in transform)
            {
                Vector2 pos =treePosition + offset + delta * i++;
                var node = trans.GetComponent<TechnologyNode>();
                if (node != null) node.treePosition = pos;
                else
                {
                    var regNode = trans.GetComponent<RegimentNode>();
                    if (regNode != null) regNode.treePosition = pos;
                }

            }
        }
        
    }
    static int[] childs = new int[10];
    int MaxChild()
    {
        for (int i = 0; i < 10; i++)
            childs[i] = 0;
        DFS(0, transform);
        int max = 0;
        for (int i = 0; i < 10; i++)
            max = Mathf.Max(childs[i], max);
        return max;
    }
    void DFS(int deep, Transform node)
    {
        childs[deep] += node.childCount;
        foreach (Transform tr in node)
            DFS(deep + 1, tr);
    }
}

