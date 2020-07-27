using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class RegimentNode : MonoBehaviour
{
#if UNITY_EDITOR
    public int NodeID = -1;
    public bool researched = false;
    public BaseRegiment regInst;
    public int regimentOffset;
    public RegimentTeches RegimentTeches;
    [HideInInspector] public Technology[] regTeches;
    [HideInInspector] public Technology researchRegiment;
    [HideInInspector] public Vector2 treePosition;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (regInst != null && (TechnologyTreeBuilder.build))
        {
            name = string.Format("{0}_{1} ({2}, {3}, {4}, {5})", regInst.name.ToString(), regimentOffset, regInst.damageLvl, regInst.MeleeArmor, regInst.ChargeArmor, regInst.RangeArmor);
            var attack = ScriptableObject.CreateInstance<AttackTechnology>();
            attack.lvl = regInst.damageLvl;
            
            var mellee = ScriptableObject.CreateInstance<ArmorTechnology>();
            mellee.lvl = regInst.MeleeArmor;
            mellee.armorType = DamageType.Melee;
            var charge = ScriptableObject.CreateInstance<ArmorTechnology>();
            charge.lvl = regInst.ChargeArmor;
            charge.armorType = DamageType.Charge;
            var range = ScriptableObject.CreateInstance<ArmorTechnology>();
            range.lvl = regInst.RangeArmor;
            range.armorType = DamageType.Range;
            mellee.regimentOffset = charge.regimentOffset = range.regimentOffset = attack.regimentOffset = regimentOffset;
            mellee.lvlmax = charge.lvlmax = range.lvlmax = attack.lvlmax = GameConst.MaxRegimentLevel;


            regTeches = new Technology[4] { attack, mellee, charge, range };

            if (!researched)
            {
                var newReg = ScriptableObject.CreateInstance<NewRegimentTechnology>();
                newReg.regimentInstance = regInst;
                newReg.regimentOffset = regimentOffset;
                researchRegiment = newReg;

                AssetDatabase.CreateAsset(newReg, string.Format("Assets/Resources/Technology/Regiments/{0}_{1}.asset", regInst.name, "newReg"));
                EditorUtility.SetDirty(newReg);

            }
            else
                researchRegiment = null;

            if (TechnologyTreeBuilder.PanelTransform != null)
            {
                if (RegimentTeches == null)
                {
                    RegimentTeches = Instantiate(PrefabHandler.RegimentTechnologyPanelPrefab, TechnologyTreeBuilder.PanelTransform);

                }
                RegimentTeches.transform.SetAsLastSibling();
                NodeID = TechnologyTreeBuilder.PanelTransform.childCount - 1;
                RegimentTeches.name = NodeID.ToString();
                RegimentTeches.GetComponent<RectTransform>().anchoredPosition = treePosition;
                RegimentTeches.Init(regInst, regTeches, researchRegiment);
                mellee.NodeID = charge.NodeID = range.NodeID = attack.NodeID = NodeID;
                if (researchRegiment) researchRegiment.NodeID = NodeID;
            }
            AssetDatabase.CreateAsset(attack, string.Format("Assets/Resources/Technology/Regiments/{0}_{1}.asset", regInst.name, "attack"));
            AssetDatabase.CreateAsset(mellee, string.Format("Assets/Resources/Technology/Regiments/{0}_{1}.asset", regInst.name, "mellee"));
            AssetDatabase.CreateAsset(charge, string.Format("Assets/Resources/Technology/Regiments/{0}_{1}.asset", regInst.name, "charge"));
            AssetDatabase.CreateAsset(range, string.Format("Assets/Resources/Technology/Regiments/{0}_{1}.asset", regInst.name, "range"));
            EditorUtility.SetDirty(attack);
            EditorUtility.SetDirty(mellee);
            EditorUtility.SetDirty(charge);
            EditorUtility.SetDirty(range);

        }
        else
            name = "null";
    }
#endif
}
