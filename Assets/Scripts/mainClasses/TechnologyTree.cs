using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechnologySystem
{
    public class TechnologyTree : ScriptableObject
    {
        public const int maximumEra = 3;
        public int technologyEra = 0;

        //military
        public BaseRegiment[] regiments;
        public bool canBuildWalls = false;
        public float wallsCostReduce = 1;
        public float moveSpeedBonus = 1;
        //economica
        public bool canBuildTrade = false;
        public bool canBuildPort = false;
        public float portTradeBonus = 1;
        public float regimentCostReduce = 1;
        public float regimentUpkeepReduce = 1;
        public float recruitInCountry = GameConst.RecruitInCountry;
        public float recruitInTown = GameConst.RecruitInTown;
        public Treasury treasureIncrease = new Treasury(1);

        //government
        public bool canBuildInfrastructure = false;
        public float passiveExperience = 0;
        public float activeExperienceBonus = 1;
        public float inquisitionBonus = 0;
        public int maxPerson = 1;
        public float burnedLandEffect = 0;
        public List<PersonType> openedPerson;


        //other parametrs//
        public State state;
        public List<Technology> technology = new List<Technology>();
        public List<int> parents = new List<int>();
        public List<List<int>> childs = new List<List<int>>();

        public void InitializeTree(State state)
        {
            this.state = state;
            state.technologyTree = this;
            regiments = state.regiments;
            openedPerson = new List<PersonType>();

            BuildTree(0);
        }
        private void BuildTree(int index)
        {
            Technology tech = technology[index] = Instantiate(technology[index]);

            int lvl = tech.lvl;
            tech.lvl = 0;
            while (tech.lvl++ != lvl)
                tech.LevelUp();
            if(childs[index] != null)
            foreach (int child in childs[index])
                BuildTree(child);
        }
        public void TechnologyResearched(Technology technology) { }
    }

    public class Technology : ScriptableObject
    {
        public float science, time;
        public int lvl = 0, lvlmax = 1;
        public new string name;
        public string description;
        public GameAction researchAction = null;
        public System.Action buttonAction;
        [HideInInspector] public Technology parent = null;
        [HideInInspector] public TechnologyTree tree;
        public ResearchType researchType => researchAction != null ? ResearchType.Researching :
            (lvl < lvlmax && (parent == null || parent.lvl != 0) && lvl * TechnologyTree.maximumEra < tree.technologyEra * lvlmax) ? ResearchType.CanResearch :
            lvl == lvlmax ? ResearchType.Limit : ResearchType.Blocked;
        public void Research()
        {
            lvl++;
            LevelUp();
            buttonAction?.Invoke();
            researchAction = null;
            tree.TechnologyResearched(this);
        }
        public virtual void LevelUp()
        {

        }
    }
    
    public enum ResearchType
    {
        Researching,
        CanResearch,
        Blocked,
        Limit
    }
    [CreateAssetMenu(fileName = "EraTech", menuName = "Technology/EraTech", order = 1)]
    public class EraTechnology : Technology
    {
        public override void LevelUp() => tree.technologyEra++;
    }

    [CreateAssetMenu(fileName = "AttackTech", menuName = "Technology/AttackTech", order = 1)]
    public class AttackTechnology : Technology
    {
        public int regimentOffset;
        public override void LevelUp() => tree.regiments[regimentOffset].damageLvl++;
    }

    [CreateAssetMenu(fileName = "ArmorTech", menuName = "Technology/ArmorTech", order = 1)]
    public class ArmorTechnology : Technology
    {
        public int regimentOffset;
        public DamageType armorType;
        public override void LevelUp()
        {
            switch (armorType)
            {
                case DamageType.Range: tree.regiments[regimentOffset].RangeArmor++; break;
                case DamageType.Melee: tree.regiments[regimentOffset].MeleeArmor++; break;
                case DamageType.Charge: tree.regiments[regimentOffset].ChargeArmor++; break;
            }
        }
    }
    [CreateAssetMenu(fileName = "NewRegimentTech", menuName = "Technology/NewRegimentTech", order = 1)]
    public class NewRegimentTechnology : Technology
    {
        public string regimentName;
        public int regimentOffset;
        public RegimentType movementType;
        public DamageType damageType;
        //public override void LevelUp() => tree.regiments[regimentOffset] = 
         //   new BaseRegiment(tree.state, RegimentName.SimpleArta, movementType, damageType,)
    }
    [CreateAssetMenu(fileName = "CanBuildInfrastructureTech", menuName = "Technology/CanBuildInfrastructureTech", order = 1)]
    public class CanBuildInfrastructureTechnology : Technology
    {
        public override void LevelUp() => tree.canBuildInfrastructure = true;
    }

    [CreateAssetMenu(fileName = "CanBuildPortTech", menuName = "Technology/CanBuildPortTech", order = 1)]
    public class CanBuildPortTechnology : Technology
    {
        public override void LevelUp() => tree.canBuildPort = true;
    }

    [CreateAssetMenu(fileName = "CanBuildTradeTech", menuName = "Technology/CanBuildTradeTech", order = 1)]
    public class CanBuildTradeTechnology : Technology
    {
        public override void LevelUp() => tree.canBuildTrade = true;
    }

    [CreateAssetMenu(fileName = "CanBuildWallsTech", menuName = "Technology/CanBuildWallsTech", order = 1)]
    public class CanBuildWallsTechnology : Technology
    {
        public override void LevelUp() => tree.canBuildWalls = true;
    }

    [CreateAssetMenu(fileName = "WallsCostReduceTech", menuName = "Technology/WallsCostReduceTech", order = 1)]
    public class WallsCostReduceTechnology : Technology
    {
        public float[] percent;
        public override void LevelUp() { tree.wallsCostReduce = percent[lvl - 1]; }
    }

    [CreateAssetMenu(fileName = "MoveSpeedBonusTech", menuName = "Technology/MoveSpeedBonusTech", order = 1)]
    public class MoveSpeedBonusTechnology : Technology
    {
        public float moveSpeedBonus;
        public override void LevelUp() => tree.moveSpeedBonus = moveSpeedBonus;
    }
}
