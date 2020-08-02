using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/*
[ExecuteInEditMode]
public class TechnologyTreeBuilder : MonoBehaviour
{
    public static bool build => instance != null && instance.onEnable;
    static TechnologyTreeBuilder instance;
    public bool onEnable = false;
    public Transform PanelPater;
    public static Transform PanelTransform;
    private void Awake()
    {
        onEnable = false;
    }
    private void Update()
    {
        instance = this;
        if (PanelPater != null)
            PanelTransform = PanelPater;
        if (!onEnable)
            return;
        TechnologyTree tree = ScriptableObject.CreateInstance<TechnologyTree>();
        BuildTree(tree, GetComponent<TechnologyNode>(), -1);
        AssetDatabase.CreateAsset(tree, "Assets/Resources/Technology/Trees/tree.asset");
    }
    private void BuildTree(TechnologyTree tree, TechnologyNode node, int parent)
    {
        if (node == null || node.technology == null)
            return;
        int id = tree.technology.Count;
        tree.technology.Add(node.technology);
        node.technology.NodeID = node.NodeID;
        tree.parents.Add(parent);
        foreach (Transform son in node.transform)
        {
            TechnologyNode next = son.GetComponent<TechnologyNode>();
            if (next != null)
            {
                if (next.technology != null)
                {
                    BuildTree(tree, next, id);
                }
            } else
            {
                RegimentNode regNode = son.GetComponent<RegimentNode>();
                if(regNode != null)
                {
                    int realId = id;
                    if(!regNode.researched)
                    {
                        realId = tree.technology.Count;
                        regNode.researchRegiment.NodeID = regNode.NodeID;
                        tree.technology.Add(regNode.researchRegiment);
                        tree.parents.Add(id);
                    }
                    int i0 = tree.technology.Count;

                    tree.technology.Add(regNode.regTeches[0]);
                    tree.technology.Add(regNode.regTeches[1]);
                    tree.technology.Add(regNode.regTeches[2]);
                    tree.technology.Add(regNode.regTeches[3]);
                    tree.parents.Add(realId);
                    tree.parents.Add(realId);
                    tree.parents.Add(realId);
                    tree.parents.Add(realId);
                }
            }

        }
    }
}
*/