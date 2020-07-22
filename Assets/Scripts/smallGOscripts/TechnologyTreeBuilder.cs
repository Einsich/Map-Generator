using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TechnologySystem;
[ExecuteInEditMode]
public class TechnologyTreeBuilder : MonoBehaviour
{

    public bool onEnable = false;
    private void Update()
    {
        if (!onEnable)
            return;
        TechnologyTree tree = ScriptableObject.CreateInstance<TechnologyTree>();
        TechnologyNode.nodeIDCounter = 0;
        BuildTree(tree, GetComponent<TechnologyNode>(), -1);
        AssetDatabase.CreateAsset(tree, "Assets/Resources/Technology/Trees/tree.asset");
    }
    private void BuildTree(TechnologyTree tree, TechnologyNode node, int parent)
    {
        if (node == null || node.technology == null)
            return;
        int id = node.getID;
        tree.technology.Add(node.technology);
        tree.parents.Add(parent);
        List<int> childs = new List<int>();
        tree.childs.Add(childs);
        foreach (Transform son in node.transform)
        {
            TechnologyNode next = son.GetComponent<TechnologyNode>();
            if (next != null)
            {
                if (next.technology != null)
                {
                    childs.Add(next.getID);
                    BuildTree(tree, next, id);
                }
            } else
            {
                RegimentNode regNode = son.GetComponent<RegimentNode>();
                if(regNode != null)
                {
                    var attack = ScriptableObject.CreateInstance<TechnologySystem.AttackTechnology>();
                    attack.lvl = regNode.Attack;
                    var mellee = ScriptableObject.CreateInstance<TechnologySystem.ArmorTechnology>();
                    mellee.lvl = regNode.Attack;
                    mellee.armorType = DamageType.Melee;
                    var charge = ScriptableObject.CreateInstance<TechnologySystem.ArmorTechnology>();
                    charge.lvl = regNode.Attack;
                    charge.armorType = DamageType.Charge;
                    var range = ScriptableObject.CreateInstance<TechnologySystem.ArmorTechnology>();
                    range.lvl = regNode.Attack;
                    range.armorType = DamageType.Range;
                    mellee.regimentOffset = charge.regimentOffset = range.regimentOffset =
                        attack.regimentOffset = regNode.regimentOffset;
                    tree.technology.Add(attack);
                    tree.technology.Add(mellee);
                    tree.technology.Add(charge);
                    tree.technology.Add(range);
                    tree.parents.Add(id);
                    tree.parents.Add(id);
                    tree.parents.Add(id);
                    tree.parents.Add(id);
                }
            }

        }
    }
}
