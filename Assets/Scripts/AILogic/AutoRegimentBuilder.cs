using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AutoRegimentBuilder : AutoManager, AutoSpender
{
    private StateAI stateAI;
    public HashSet<RegionProxi> RiskI, RiskII, RiskIII;

    //private Dictionary<int, Dictionary<RegimentIdentifier, float>> templates;
    private static int ID_PROV_TEMPLATE;

    private float FIRST_LINE_PIECE = 4;
    private float TWO_LINE_PIECE = 2;

    //private Dictionary<RegimentIdentifier, float> needRegiment = new Dictionary<RegimentIdentifier, float>();
    //private Dictionary<RegimentIdentifier, BaseRegiment> stateRegiments = new Dictionary<RegimentIdentifier, BaseRegiment>();
    private List<BaseRegiment> stateRegiments = new List<BaseRegiment>();
    private PriorityQueue<RegimentBuildTask> priorityQueue = new PriorityQueue<RegimentBuildTask>();
    private bool isPeace => stateAI.Data.diplomacy.war.Count == 0;
    private Treasury dirtyIncome=>stateAI.Data.Income - stateAI.Data.allRegimentsUpkeep;

    private int updateRegimentBase = 0;
    static AutoRegimentBuilder()
    {
        ID_PROV_TEMPLATE = (int)PersonType.Count;
    }

    public AutoRegimentBuilder(StateAI aI)
    {
        (stateAI, RiskI, RiskII, RiskIII) = (aI,new HashSet<RegionProxi>(), new HashSet<RegionProxi>(), new HashSet<RegionProxi>());
        //CreateTemplates();
    }

   /* private void CreateTemplates()
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
    */
    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {
            if (isOn == value)
                return;
            isOn = value;
            if (value)
            {
                stateAI.ProcessOnlyBuildAndRegimnts();
            }
            else
            {
            }
        }
    }

    private void AutoBuildRegiment()
    {
        if (updateRegimentBase-- <= 0)
        {
            updateRegimentBase = 5;
            AddBase();
            if (isPeace)
            {
                AnalizeRegions((r, neib) => neib.curOwner != stateAI.Data);
                //CompleteArmy();
                CompletingProv(RiskI, 2, 3);
                CompletingProv(RiskII, 2.7f, 1);
                CompletingProv(RiskIII, 5, 0);
            }
            else
            {
                AnalizeRegions((r, neib) => neib.owner != null && neib.owner.diplomacy.haveWar(stateAI.Data.diplomacy));
                //CompleteArmy();
                CompletingProv(RiskI, 1, 3);
                CompletingProv(RiskII, 1.7f, 1);
                CompletingProv(RiskIII, 5, 0);
            }
        }
    }


    public void AnalizeRegions(System.Func<Region, Region, bool> constructRiskI)
    {
        RiskI.Clear();
        RiskII.Clear();
        RiskIII.Clear();

        var regs = stateAI.Data.regions;
        void Func(HashSet<RegionProxi> sets, System.Func<Region, Region, bool> pred)
        {
            foreach (var r in regs)
                if(r.curOwner == stateAI.Data)
                foreach (var neib in r.neib)
                    if (pred(r, neib))
                    {
                        sets.Add(new RegionProxi(r.data));
                        break;
                    }
        }

        Func(RiskI, constructRiskI);
        Func(RiskII, (r, neib) => RiskI.Contains(new RegionProxi(neib.data)) && !RiskI.Contains(new RegionProxi(r.data)));
        Func(RiskIII, (r, neib) => !RiskI.Contains(new RegionProxi(r.data)) && !RiskII.Contains(new RegionProxi(r.data)));

    }
    
    private void AddBase()
    {
        stateRegiments.Clear();

        foreach (BaseRegiment baseRegiment in stateAI.Data.regiments)
        {
            if (baseRegiment == null)
                continue;
            stateRegiments.Add(baseRegiment);
            //var key = new RegimentIdentifier(baseRegiment.type, baseRegiment.damageType);
            //if (!stateRegiments.ContainsKey(key))
            //    stateRegiments.Add(key, baseRegiment);
        }
    }
    /*
    private void CompleteArmy()
    {
        foreach (Army a in stateAI.Data.army)
        {
            if (a.inTown && a.curReg.curOwner == a.owner)
            {
                var needRegiment = CompletionRegimentList(a.army, templates[(int)a.Person.personType]);

                int needNumber = a.Person.MaxRegiment - a.army.Count;

                for (int i = a.curReg.data.garnison.Count - 1; i >= 0; i--)
                {
                    Regiment regiment = a.curReg.data.garnison[i];
                    BaseRegiment baseRegiment = regiment.baseRegiment;
                    var regID = new RegimentIdentifier(baseRegiment.type, baseRegiment.damageType);

                    if (needNumber > 0 && needRegiment.ContainsKey(regID) && needRegiment[regID] > 0)
                    {
                        
                        Debug.Log("removable: " + regiment);
                        a.ExchangeRegiment(regiment);
                        needNumber--;
                        needRegiment[regID]--;
                       
                    }
                }

                foreach (var nr in needRegiment)
                {
                    ProvinceData prov = a.curReg.data;
                    BaseRegiment regiment = stateRegiments[nr.Key];
                    Treasury cost = regiment.cost;
                    Treasury upkeep = regiment.upkeep;

                    for (float i = 0; i < nr.Value; i++)
                    {
                        if (needNumber > 0)
                        {
                            priorityQueue.Enqueue(new RegimentBuildTask(prov, regiment, 1 + UnityEngine.Random.Range(0f, 1f)));
                            needNumber--;
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
  */

    private void CompletingProv(HashSet<RegionProxi> risk, float Priority, int wallsLvl)
    {

        foreach (RegionProxi proxi in risk)
        {
            ProvinceData prov = proxi.data;
            AutoBuilder builder = stateAI.autoBuilder;

            if (builder.queue.Count == 0 ||
                (prov.wallsLevel < wallsLvl && builder.queue.Peek().Building != BuildingType.Walls))
            {
                builder.IncludeBuildTask(prov, BuildingType.Walls);
            }
            int need = prov.maxGarnison - prov.garnison.Count;
            //foreach (var nr in CompletionRegimentList(prov.garnison, templates[ID_PROV_TEMPLATE]))
            for(int i = need; i >= 0; i--)
            {
                //BaseRegiment regiment = stateRegiments[nr.Key];
                BaseRegiment regiment = stateRegiments[UnityEngine.Random.Range(0, stateRegiments.Count)];
                priorityQueue.Enqueue(new RegimentBuildTask(prov, regiment, Priority + UnityEngine.Random.Range(0f, 1f)));
                /*
                for (float i = 0; i < nr.Value; i++)
                {
                    if (need-- > 0)
                        priorityQueue.Enqueue(new RegimentBuildTask(prov, regiment, Priority + UnityEngine.Random.Range(0f, 1f)));
                    else
                        break;
                }*/
            }
        }
    }

    public AutoSpenderResult TrySpend(StateAI state)
    {
        AutoBuildRegiment();
        if (priorityQueue.Count > 0)
        {
            var task = priorityQueue.Peek();
            if (!(dirtyIncome.NormalizedToGold >= task.baseRegiment.upkeep.NormalizedToGold && task.province.region.curOwner == state.Data))
                return AutoSpenderResult.HasNotOrder;
            if (state.treasury >= task.baseRegiment.cost)
            {
                task.province.AddRecruit(new RecruitAction(task.province, task.baseRegiment, task.baseRegiment.time));
                priorityQueue.Dequeue();
                return AutoSpenderResult.Success;
            }
            else
                return AutoSpenderResult.NeedMoreResources;
        } else
            return AutoSpenderResult.HasNotOrder;
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
    public struct RegimentBuildTask : IComparable<RegimentBuildTask>
    {
        public ProvinceData province;
        public BaseRegiment baseRegiment;
        private float priority;
        public RegimentBuildTask(ProvinceData province, BaseRegiment baseRegiment, float priority) =>
            (this.province, this.baseRegiment, this.priority) = (province, baseRegiment, priority);
        public int CompareTo(RegimentBuildTask other)
        {
            return priority.CompareTo(other.priority);
        }
    }
}
