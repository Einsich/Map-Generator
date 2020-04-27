using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRegimentBuilder : AutoManager
{
    private StateAI stateAI;
    private HashSet<RegionProxi> RiskI, RiskII, RiskIII;
    public Treasury NeedTreasure => new Treasury();

    private Dictionary<int, Template> templates;
    private static int ID_PROV_TEMPLATE;

    private int FIRST_LINE_PIECE = 1;
    private int TWO_LINE_PIECE = 2;

    private bool isPeace = true;
    static AutoRegimentBuilder()
    {
        ID_PROV_TEMPLATE = (int)PersonType.Count;
    }

    public AutoRegimentBuilder(StateAI aI)
    {
        (stateAI, RiskI, RiskII, RiskIII) = (aI,new HashSet<RegionProxi>(), new HashSet<RegionProxi>(), new HashSet<RegionProxi>());
        CreateTemplates();
    }

    private void CreateTemplates()
    {
        templates = new Dictionary<int, Template>();

        int templateID;
        for (PersonType person = PersonType.Warrior; person != PersonType.Count; person++)
        {
            templateID = (int)person;
            templates.Add(templateID, new Template(new List<TemplateElement>()
            {
                new TemplateElement(Random.Range(1, 4), RegimentType.Infantry, DamageType.Melee),
                new TemplateElement(Random.Range(1, 4), RegimentType.Infantry, DamageType.Range),
                new TemplateElement(Random.Range(1, 3), RegimentType.Cavalry, DamageType.Charge),
                new TemplateElement(Random.Range(1, 3), RegimentType.Artillery, DamageType.Siege)
            }));
        }

        templates.Add(ID_PROV_TEMPLATE, new Template(new List<TemplateElement>()
            {
                new TemplateElement(1, RegimentType.Infantry, DamageType.Melee)
            }));
    }

    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {
            if (value)
            {
                AutoBuildRegiment();
                GameTimer.AddListener(AutoBuildRegiment, stateAI.Data);
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

        Treasury clearIncome = Treasury.zero;
        foreach (Region reg in stateAI.Data.regions)
        {
            ProvinceData prov = reg.data;
            clearIncome += prov.income;
        }

        Template townTemplate = templates[ID_PROV_TEMPLATE];
        if (townTemplate.Upkeep.Gold == 0)
        {
            townTemplate.SearchRegimentForBuild(stateAI.Data);
        }

        int countRiskIV = stateAI.Data.regions.Count - RiskI.Count - RiskII.Count - RiskIII.Count;
        int piecesForGarnisons = RiskI.Count * FIRST_LINE_PIECE + (RiskII.Count + RiskIII.Count) * TWO_LINE_PIECE + countRiskIV;

        Treasury pieceUpkeep = clearIncome * (2 / (float)piecesForGarnisons);

        Treasury riskIUpkeep = pieceUpkeep * 4;
        foreach (RegionProxi proxi in RiskI)
        {
            ProvinceData prov = proxi.data;
            
            foreach (TemplateElement e in townTemplate.MatchedElements)
            {

            }
        }
        if (isPeace)
        {
            CheckAndChangeBudget(0.15f);

            Treasury personsRegimentUpkeep = Treasury.zero;
            foreach (Army army in stateAI.Data.army)
            {
                Template template = templates[(int)army.Person.personType];
                float templateOccurred = Mathf.Floor(army.Person.MaxRegiment * 0.5f / template.RegimentCount);


            }

            personsRegimentUpkeep *= 0.5f; //D = Dбаз (t 0.5 + (1 - t))

        }
        else
        {
            CheckAndChangeBudget(0.95f);
        }
    }

    private void CompletionRegimentList(Treasury budget, List<Regiment> regiments, Template template)
    {
        var zero = Treasury.zero;

        if (regiments.Count > 0)
        {
            foreach (Regiment r in regiments)
            {

            }
        }
    }

    private void BuildingLotRegiment(int required, ProvinceData prov,BaseRegiment regiment)
    {
        for (int i = 0; i < required; i++)
        {
            prov.recruitQueue.Add(new RecruitAction(prov, regiment, regiment.time));
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

    private int NumberOfOccurre(Treasury total, Treasury piece)
    {
        piece.Gold = ZeroToOne(piece.Gold);
        piece.Manpower = ZeroToOne(piece.Manpower);
        piece.Iron = ZeroToOne(piece.Iron);
        piece.Wood = ZeroToOne(piece.Wood);
        piece.Science = ZeroToOne(piece.Science);

        float[] occurre = new float[5];
        occurre[0] = total.Gold / piece.Gold;
        occurre[1] = total.Manpower / piece.Manpower;
        occurre[2] = total.Iron / piece.Iron;
        occurre[3] = total.Wood / piece.Wood;
        occurre[4] = total.Science / piece.Science;

        float min = Mathf.Min(occurre);
        return (int)Mathf.Floor(min);
    }
    
    private float ZeroToOne(float value)
    {
        return value == 0 ? 1 : value;
    }
    public void BuildWalls()
    {
        foreach (RegionProxi provProxi in RiskI)
        {
            provProxi.data.recruitQueue.Add(new RecruitAction(provProxi.data, stateAI.Data.melee, stateAI.Data.melee.time));
            provProxi.data.recruitQueue.Add(new RecruitAction(provProxi.data, stateAI.Data.melee, stateAI.Data.melee.time));
            provProxi.data.recruitQueue.Add(new RecruitAction(provProxi.data, stateAI.Data.melee, stateAI.Data.melee.time));
            provProxi.data.recruitQueue.Add(new RecruitAction(provProxi.data, stateAI.Data.melee, stateAI.Data.melee.time));
            provProxi.data.recruitQueue.Add(new RecruitAction(provProxi.data, stateAI.Data.melee, stateAI.Data.melee.time));

            if (provProxi.data.wallsLevel <= 4)
            {
                stateAI.autoBuilder.IncludeBuildTask(provProxi.data, BuildingType.Walls);
            }
        }
    }

    private class Template
    {
        private List<TemplateElement> notMatchedElements;
        public List<TemplateElement> MatchedElements { get; }

        public Treasury Upkeep { get; private set; }
        public Treasury Cost { get; private set; }
        public int RegimentCount { get; private set; }

        public Template(List<TemplateElement> templateElement)
        {
            notMatchedElements = templateElement;
            MatchedElements = new List<TemplateElement>();
            Upkeep = Treasury.zero;
            Cost = Treasury.zero;
            RegimentCount = 0;
        }

        public void SearchRegimentForBuild(State state)
        {
            foreach (TemplateElement templ in notMatchedElements)
            {
                foreach (BaseRegiment r in state.regiments)
                {
                    if (r.damageType == templ.DamageType && r.type == templ.RegimentType)
                    {
                        templ.matchInState = r;
                        MatchedElements.Add(templ);
                        //notMatchedElements.Remove(templ);

                        Upkeep += r.upkeep;
                        Cost += r.cost;
                        RegimentCount += templ.Factor;
                    }
                }
            }
        }
    }

    private class TemplateElement
    {
        public int Factor { get; }
        public DamageType DamageType { get; }
        public RegimentType RegimentType { get; }

        public BaseRegiment matchInState { get; set; }

        public TemplateElement(int factor, RegimentType regimentType, DamageType damageType)
        {
            Factor = factor;
            this.RegimentType = regimentType;
            this.DamageType = damageType;
            matchInState = null;
        }
    }

    private struct RegionProxi
    {
        public ProvinceData data;
        public RegionProxi(ProvinceData provinceData) =>
             (data) = (provinceData);
    }
}
