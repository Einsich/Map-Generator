using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRegimentBuilder : AutoManager
{
    private StateAI stateAI;
    private HashSet<RegionProxi> RiskI, RiskII, RiskIII;
    public Treasury NeedTreasure => new Treasury();

    public AutoRegimentBuilder(StateAI aI)
    {
        (stateAI, RiskI, RiskII, RiskIII) = (aI,new HashSet<RegionProxi>(), new HashSet<RegionProxi>(), new HashSet<RegionProxi>());
        
    }


    private bool isOn = false;
    public bool IsOn
    {
        get => isOn; set
        {
            if (value)
            {
                AnalizeRegions();
            }
            else
            {

            }
            isOn = value;
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
        Func(RiskII, (r, neib) => RiskI.Contains(new RegionProxi(neib.data)));
        Func(RiskIII, (r, neib) => !RiskI.Contains(new RegionProxi(neib.data)) || !RiskII.Contains(new RegionProxi(neib.data)));
    }


    private struct RegionProxi
    {
        public ProvinceData data;
        public RegionProxi(ProvinceData provinceData) =>
             (data) = (provinceData);
    }
}
