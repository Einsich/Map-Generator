using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRegimentBuilder : AutoManager
{
    private StateAI stateAI;
    private HashSet<RegionProxi> RiskI, RiskII, RiskIII;
    public Treasury NeedTreasure => new Treasury();

    private Dictionary<PersonType, List<RecruitTemplate>> personTemplates;
    private List<RecruitTemplate> provinceTemplate;

    private bool isPeace = true;
    static AutoRegimentBuilder()
    {

    }

    public AutoRegimentBuilder(StateAI aI)
    {
        (stateAI, RiskI, RiskII, RiskIII) = (aI,new HashSet<RegionProxi>(), new HashSet<RegionProxi>(), new HashSet<RegionProxi>());
        CreateTemplates();
    }

    private void CreateTemplates()
    {
        provinceTemplate = new List<RecruitTemplate>()
        {
            new RecruitTemplate(3, RegimentType.Infantry, DamageType.Melee),
            new RecruitTemplate(2, RegimentType.Infantry, DamageType.Range),
            new RecruitTemplate(2, RegimentType.Cavalry, DamageType.Charge),
            new RecruitTemplate(1, RegimentType.Artillery, DamageType.Siege)
        };

        personTemplates = new Dictionary<PersonType, List<RecruitTemplate>>();
        for (PersonType person = PersonType.Warrior; person != PersonType.Count; person++)
        {
            personTemplates.Add(person, new List<RecruitTemplate>()
        {
            new RecruitTemplate(Random.Range(1, 4), RegimentType.Infantry, DamageType.Melee),
            new RecruitTemplate(Random.Range(1, 4), RegimentType.Infantry, DamageType.Range),
            new RecruitTemplate(Random.Range(1, 3), RegimentType.Cavalry, DamageType.Charge),
            new RecruitTemplate(Random.Range(1, 3), RegimentType.Artillery, DamageType.Siege)
        });
        }
    }

    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {
            if (value)
            {
                AutoBuildRegiment();
                //GameTimer.AddListener(BuildWalls, stateAI.Data);
            }
            else
            {

            }
            isOn = value;
        }
    }

    private void AutoBuildRegiment()
    {
        AnalizeRegions();

        if (isPeace)
        {
            CheckAndChangeBudget(0.15f);

            foreach (RegionProxi provP in RiskI)
            {

            }
        }
        else
        {
            CheckAndChangeBudget(0.95f);
        }
    }

    private void CheckAndChangeBudget(float newValue)
    {
        if (stateAI.armyBudget != newValue)
        {
            stateAI.ChangeBudget(newValue, BudgetType.ArmyBudget);
        }
    }
    public void AnalizeRegions()
    {
        RiskI.Clear();
        RiskII.Clear();
        RiskIII.Clear();

        var regs = stateAI.Data.regions;
        void Func(HashSet<RegionProxi> sets, System.Func<Region, Region, bool> pred)
        {

            foreach (var r in regs)
                foreach (var neib in r.neib)
                    if (pred(r, neib))
                    {
                        sets.Add(new RegionProxi(r.data));
                        break;
                    }
        }
        Func(RiskI, (r, neib) => neib.owner != stateAI.Data);
        Func(RiskII, (r, neib) => !RiskI.Contains(new RegionProxi(neib.data)));
        Func(RiskIII, (r, neib) => !RiskI.Contains(new RegionProxi(neib.data)) || !RiskII.Contains(new RegionProxi(neib.data)));
    }

    public void BuildWalls()
    {
        foreach (RegionProxi provProxi in RiskI)
        {
            if (provProxi.data.wallsLevel <= 4)
            {
                stateAI.autoBuilder.IncludeBuildTask(provProxi.data, BuildingType.Walls);
            }
        }
    }

    private class RecruitTemplate
    {
        public int Factor { get; private set; }
        private DamageType damageType;
        private RegimentType regimentType;

        private BaseRegiment matchInState;

        public RecruitTemplate(int factor, RegimentType regimentType, DamageType damageType)
        {
            Factor = factor;
            this.regimentType = regimentType;
            this.damageType = damageType;
            matchInState = null;
        }

        public BaseRegiment GetBaseRegiment(State state)
        {
            if (matchInState == null)
            {
                foreach (BaseRegiment r in state.regiments)
                {
                    if (r.damageType == damageType && r.type == regimentType)
                    {
                        matchInState = r;
                        break;
                    }
                }
            }
            return matchInState;
        }
    }

    private struct RegionProxi
    {
        public ProvinceData data;
        public RegionProxi(ProvinceData provinceData) =>
             (data) = (provinceData);
    }
}
