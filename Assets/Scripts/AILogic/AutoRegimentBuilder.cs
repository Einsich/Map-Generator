using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AutoRegimentBuilder : AutoManager
{
    private StateAI stateAI;
    private HashSet<RegionProxi> RiskI, RiskII, RiskIII;
    public Treasury NeedTreasure => deficit;

    private Dictionary<int, Dictionary<RegimentIdentifier, float>> templates;
    private static int ID_PROV_TEMPLATE;

    private float FIRST_LINE_PIECE = 4;
    private float TWO_LINE_PIECE = 2;

    private Dictionary<RegimentIdentifier, float> needRegiment = new Dictionary<RegimentIdentifier, float>();
    private Dictionary<RegimentIdentifier, BaseRegiment> stateRegiments = new Dictionary<RegimentIdentifier, BaseRegiment>();

    private bool isPeace => stateAI.Data.diplomacy.war.Count == 0;
    private Treasury armyIncome, armyBudget, pieceUpkeep, pieceBudget;
    private Treasury zero = Treasury.zero;
    private Treasury deficit = Treasury.zero;
    private float x = 1;
    private int updateRegimentBase = 0;
    private int updateRegion = 0;
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
        templates = new Dictionary<int, Dictionary<RegimentIdentifier, float>>();

        int templateID;
        for (PersonType person = PersonType.Warrior; person != PersonType.Count; person++)
        {
            templateID = (int)person;
            templates.Add(templateID, new Dictionary<RegimentIdentifier, float>());
            templates[templateID].Add(new RegimentIdentifier(RegimentType.Infantry, DamageType.Melee), 1);
            templates[templateID].Add(new RegimentIdentifier(RegimentType.Infantry, DamageType.Range), 2);
        }

        templates.Add(ID_PROV_TEMPLATE, new Dictionary<RegimentIdentifier, float>());
        templates[ID_PROV_TEMPLATE].Add(new RegimentIdentifier(RegimentType.Infantry, DamageType.Range), 1);
        templates[ID_PROV_TEMPLATE].Add(new RegimentIdentifier(RegimentType.Infantry, DamageType.Melee), 1);
    }

    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {
            if (isOn == value)
                return;
            if (value)
            {
                GameTimer.AddListener(AutoBuildRegiment, stateAI.Data);
            }
            else
            {
                GameTimer.RemoveListener(AutoBuildRegiment, stateAI.Data);
            }
            isOn = value;
        }
    }

    private void AutoBuildRegiment()
    {
        if (isPeace)
        {
            CheckAndChangeBudget(0.15f);
            x = 1;
        }
        else
        {
            Debug.Log("WARRRRRRRRRRRRR");
            CheckAndChangeBudget(0.95f);
            x += 0.1f;
        }

        deficit = Treasury.zero;
        armyIncome =  stateAI.Data.Income * stateAI.armyBudget - stateAI.Data.allRegimentsUpkeep;
        armyBudget = stateAI.GetArmyBudget;

        if (updateRegimentBase-- == 0)
        {
            updateRegimentBase = 5;
            AddBase();
        }

        if (updateRegion-- == 0)
        {
            updateRegion = 5;
            AnalizeRegions();
        }

        if (armyIncome >= zero)
        {
            if (isPeace)
            {
                CompleteArmy(2, UpkeepArmyInPeace);
                CalculatePiece();
                CompletingProv(FIRST_LINE_PIECE, 1, RiskI, 4);
                CompletingProv(TWO_LINE_PIECE, 1, RiskII, 1);
                CompletingProv(1, 1, RiskIII, 0);
            }
            else
            {
                CompleteArmy(1, UpkeepArmyInWar);
                CalculatePiece();
                CompletingProv(FIRST_LINE_PIECE, x, RiskI, 4);
                CompletingProv(TWO_LINE_PIECE, x, RiskII, 1);
                CompletingProv(1, x, RiskIII, 0);
            }
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
        Func(RiskII, (r, neib) => RiskI.Contains(new RegionProxi(neib.data)) && !RiskI.Contains(new RegionProxi(r.data)));
        Func(RiskIII, (r, neib) => !RiskI.Contains(new RegionProxi(r.data)) && !RiskII.Contains(new RegionProxi(r.data)));

        updateRegion = 5;
    }
    
    private void AddBase()
    {
        stateRegiments.Clear();

        foreach (BaseRegiment baseRegiment in stateAI.Data.regiments)
        {
            var key = new RegimentIdentifier(baseRegiment.type, baseRegiment.damageType);
            //if (!stateRegiments.ContainsKey(key))
                stateRegiments.Add(key, baseRegiment);
        }
    }

    private void CompleteArmy(int reductionRegiments, System.Func<Army, BaseRegiment, Treasury> upkeepArmyInDiplomacyAct)
    {
        foreach (Army a in stateAI.Data.army)
        {
            if (a.inTown && a.curReg.owner == a.owner)
            {
                var needRegiment = CompletionRegimentList(a.army, templates[(int)a.Person.personType]);

                int needNumber = a.Person.MaxRegiment / reductionRegiments - a.army.Count;

                for (int i = a.curReg.data.garnison.Count - 1; i >= 0; i--)
                {
                    Regiment regiment = a.curReg.data.garnison[i];
                    BaseRegiment baseRegiment = regiment.baseRegiment;
                    var regID = new RegimentIdentifier(baseRegiment.type, baseRegiment.damageType);

                    if (needNumber > 0 && needRegiment.ContainsKey(regID) && needRegiment[regID] > 0)
                    {
                        Treasury upkeepInGarnison = a.curReg.data.UpkeepInProvince(baseRegiment);
                        Treasury upkeepMovingToArmy = upkeepArmyInDiplomacyAct(a, baseRegiment);

                        Treasury upUpkeep = upkeepMovingToArmy - upkeepInGarnison;

                        if (armyIncome > upUpkeep)
                        {
                            Debug.Log("removable: " + regiment);
                            a.ExchangeRegiment(regiment);
                            needNumber--;
                            needRegiment[regID]--;
                            armyIncome -= upUpkeep;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                foreach (var nr in needRegiment)
                {
                    ProvinceData prov = a.curReg.data;
                    BaseRegiment regiment = stateRegiments[nr.Key];
                    Treasury cost = regiment.cost;
                    Treasury upkeep = upkeepArmyInDiplomacyAct(a, regiment);

                    for (float i = 0; i < nr.Value; i++)
                    {
                        if (needNumber > 0 &&
                            armyIncome >= upkeep)
                        {
                            if (armyBudget >= cost)
                            {
                                armyBudget -= cost;
                                armyIncome -= upkeep;
                                needNumber--;
                                prov.recruitQueue.Add(new RecruitAction(prov, regiment, regiment.time));
                            }
                            else
                            {
                                if (cost > deficit)
                                    deficit = cost;

                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private Treasury UpkeepArmyInPeace(Army a, BaseRegiment regiment)
    {
        ProvinceData prov = a.curReg.data;

        return prov.UpkeepInProvince(regiment);
    }

    private Treasury UpkeepArmyInWar(Army a, BaseRegiment regiment)
    {
        return regiment.GetBonusUpkeep();
    }

    private Dictionary<RegimentIdentifier, float> CompletionRegimentList(List<Regiment> regiments, Dictionary<RegimentIdentifier, float> template)
    {
        needRegiment.Clear();

        float templateOccurre = 0;

        foreach (Regiment r in regiments)
        {
            var key = new RegimentIdentifier(r.baseRegiment.type, r.baseRegiment.damageType);
            if (template.ContainsKey(key))
            {
                if (needRegiment.ContainsKey(key))
                {
                    float occ = Mathf.Ceil(++needRegiment[key] / template[key]);

                    if (occ > templateOccurre)
                        templateOccurre = occ;
                }
                else if (stateRegiments.ContainsKey(key))
                {
                    needRegiment.Add(key, 1);

                    if (0 == templateOccurre)
                        templateOccurre = 1;
                }
            }
        }

        foreach (var t in template)
        {
            if (needRegiment.ContainsKey(t.Key))
            {
                float tmp = templateOccurre * t.Value - needRegiment[t.Key];

                if (tmp > 0)
                {
                    needRegiment[t.Key] = tmp;
                }
                else
                {   
                    needRegiment.Remove(t.Key);
                }
            }
            else if (stateRegiments.ContainsKey(t.Key))
            {
                float tmp = templateOccurre * t.Value;

                if (tmp > 0)
                    needRegiment.Add(t.Key, tmp);
            }
        }

        if (needRegiment.Count == 0)
        {
            return template;
        }
        else
        {
            return needRegiment;
        }
    }
    private void CalculatePiece()
    {
        float piecesForProvince = RiskI.Count * FIRST_LINE_PIECE + RiskII.Count * TWO_LINE_PIECE + RiskIII.Count;
        piecesForProvince *= x;

        float pieceMultiplier = 1 / piecesForProvince;
        pieceUpkeep = armyIncome * pieceMultiplier;
        pieceBudget = armyBudget * pieceMultiplier;
    }

    private void CompletingProv(float pieces, float x, HashSet<RegionProxi> risk, int wallsLvl)
    {
        float multiplier = pieces * x;

        foreach (RegionProxi proxi in risk)
        {
            Treasury riskUpkeep = pieceUpkeep * multiplier;
            Treasury riskBudget = pieceBudget * multiplier;
            ProvinceData prov = proxi.data;
            AutoBuilder builder = stateAI.autoBuilder;

            if (builder.queue.Count == 0 ||
                (prov.wallsLevel <= wallsLvl && builder.queue.Peek().Building != BuildingType.Walls))
            {
                builder.IncludeBuildTask(prov, BuildingType.Walls);
            }

            foreach (var nr in CompletionRegimentList(prov.garnison, templates[ID_PROV_TEMPLATE]))
            {
                BaseRegiment regiment = stateRegiments[nr.Key];
                Treasury cost = regiment.cost;
                Treasury upkeep = prov.UpkeepInProvince(regiment);
                for (float i = 0; i < nr.Value; i++)
                {
                    if (riskUpkeep >= upkeep)
                    {
                        if (riskBudget >= cost)
                        {
                            riskBudget -= cost;
                            riskUpkeep -= upkeep;
                            prov.recruitQueue.Add(new RecruitAction(prov, regiment, regiment.time));
                        }
                        else
                        {
                            Treasury tmp = cost * multiplier;

                            if (tmp > deficit)
                                deficit = tmp;

                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }



    public struct RegimentIdentifier
    {
        public DamageType damageType;
        public RegimentType regimentType;

        public RegimentIdentifier(RegimentType regimentType, DamageType damageType)
        {
            this.damageType = damageType;
            this.regimentType = regimentType;
        }
    }

    public struct RegionProxi
    {
        public ProvinceData data;
        public RegionProxi(ProvinceData provinceData) =>
             (data) = (provinceData);
    }
}
