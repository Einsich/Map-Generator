using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle {

    public Regiment[,] F = new Regiment[2,2* 40];
    public Army[]army;
    /// <summary>
    /// 0 - FIRE, 1 - SHOCK 
    /// </summary>
    public int phase = 0;
    public static List<Battle> allBattles = new List<Battle>();
    public Vector3Int[] StartCount=new Vector3Int[2];
    public static void ProcessAllBattles()
    {
        allBattles.RemoveAll(battle => battle.phase == -1);
        foreach (Battle battle in allBattles)
        {
            battle.MakeBattle();
        }
    }
    public Battle(Army attacker,Army defender)
    {
        army = new Army[2];
        army[0] = attacker;
        army[1] = defender;
        attacker.curBattle = defender.curBattle = this;
        allBattles.Add(this);
        StartCount[0] = GetCount(0);
        StartCount[1] = GetCount(1);
    }
    public int[] Rand = new int[2];
    /// <summary>
    /// returns 0 - attacker vins, 1 - defender vins, -1 - in progress
    /// </summary>
    public void MakeBattle()
    {
        SetArmy(0);
        SetArmy(1);
        Rand[0] = Random.Range(0, 10);
        Rand[1] = Random.Range(0, 10);
        int Da = Rand[0], Dd = Rand[1];
        int D = army[0].genegal.pips[phase] - army[1].genegal.pips[phase];
        Da += D > 0 ? D : 0;Dd += -D > 0 ? -D : 0;
        int ia=0, id=0,tar;
        while(ia>=0||id>=0)
        {
            if((ia = NextUnit(ia, 0)) >=0)
            {
                tar = Target(ia, 0);
                if(tar>=0)
                Attack(F[0, ia], F[1, tar], Da, army[0].owner);
                ia++;
            }
            if ((id = NextUnit(id, 1)) >= 0)
            {
                tar = Target(id, 1);
                if (tar >= 0) 
                Attack(F[1, id], F[0, tar], Dd, army[1].owner);
                id++;
            }
        }
        phase^=1;
        int alivea = ProcessLoses(0);
        int alived = ProcessLoses(1);
        if(alivea==0||alived==0)
        {
            if (alivea == 0)
                EndBattle(1);
            else
                EndBattle(0);
        }
    }
    public void EndBattle(State loser)
    {
        EndBattle(loser == army[0].owner ? 1 : 0);
    }
    public void EndBattle(int teamWinner)
    {
        Region retrReg = army[teamWinner ^ 1].FindRetreatRegion();
        if(retrReg==null)
        {
            foreach (Regiment r in army[teamWinner ^ 1].army)
                r.count = 0;
        }
        if(Player.curPlayer==null)
            BattleResult.ShowResult(this, teamWinner == 0);            
        
        if(Player.curPlayer == army[0].owner)
            BattleResult.ShowResult(this, teamWinner == 0);
        if (Player.curPlayer == army[1].owner)
            BattleResult.ShowResult(this, teamWinner == 1);

        if (BattleInterface.curBattle == this)
            MenuManager.ShowBattle(null);
        army[0].EndBattle();
        army[1].EndBattle();
        phase = -1;
        if (retrReg != null)
        {
            army[teamWinner ^ 1].Retreat(MapMetrics.GetCellPosition(retrReg.Capital));
        }
        else
        {
            army[teamWinner ^ 1].DestroyArmy();
        }
    }
    int NextUnit(int cur,int team)
    {
        if (cur < 0)
            return -1;
        while (cur < 80)
            if (F[team,cur++] != null)
                return cur - 1;
        return -1;
    }
    int Target(int cur,int team)
    {
        RegimentType t = F[team, cur].baseRegiment.type;
        if (cur >= 40 && t != RegimentType.Artillery)
            return -1;
        if (cur >= 40)
            cur -= 40;
        team ^= 1;
        int r = 3;
        if (t == RegimentType.Cavalry)
            r = 5;
        else
            if (t == RegimentType.Artillery)
            r = 7;
        for (int i = 0; i < r; i++) 
        {
            int d = cur + ((i+1) / 2) * ((i & 1) == 0 ? 1 : -1);
            if (0 <= d && d < 40 && F[team, d] != null)
                return d;
        }

        return -1;
    }

    void Attack(Regiment a,Regiment d,float D,State Sa)
    {
        BaseRegiment ba = a.baseRegiment, bd = d.baseRegiment;
        float M = D + (ba.pips[2, 0] - bd.pips[2, 1] > 0 ? ba.pips[2, 0] - bd.pips[2, 1] : 0);
        D += ba.pips[phase, 0] - bd.pips[phase, 1] > 0 ? ba.pips[phase, 0] - bd.pips[phase, 1] : 0;
        D = 15 + 5 * D;
        M = 15 + 5 * M;
        float q = a.count * 0.001f * (phase == 0 ? Sa.FirePower(ba.type) : Sa.ShockPower(ba.type));
        D *= q;
        M *= q*a.maxmoral/600f;
        d.moral -= M;
        d.loses = (int)(D+0.5f);        
    }
    /// <summary>
    /// returns count of fighting regiments
    /// </summary>
    int ProcessLoses(int k)
    {
        int ans = 0;
        for (int i = 0; i < 80; i++)
            if (F[k, i] != null)
            {
                F[k, i].count -= F[k, i].loses;
                F[k, i].loses = 0;
                if (F[k, i].count < 1f)
                    F[k, i].count = 0;
                if (F[k, i].count < 1f || F[k, i].moral <= 0f)
                {
                    F[k, i].moral = 0;
                    F[k, i] = null;
                }
                else
                    ans++;
            }
        return ans;
    }
    public Vector3Int GetCount(int k)
    {
        Vector3Int c = Vector3Int.zero;
        foreach(Regiment r in army[k].army)
            switch(r.baseRegiment.type)
            {
                case RegimentType.Infantry:c.x += r.count;break;
                case RegimentType.Cavalry:c.y += r.count;break;
                case RegimentType.Artillery:c.z += r.count;break;
            }
        return c;
    }
    /// <summary>
    /// from 0 to 1
    /// </summary>
    public float GetMoral(int k)
    {
        float maxmoral = 0, moral = 0;
        foreach (Regiment r in army[k].army)
        {
            maxmoral += r.maxmoral;
            moral += r.moral;
        }
        return moral / maxmoral;
    }
    void SetArmy(int k)
    {
        int i0 = 0, i1 = 0,m;
        for (int i = 0; i < 80; i++)
            F[k, i] = null;
        m = 60;
        foreach (Regiment r in army[k].army)
            if (r.baseRegiment.type== RegimentType.Artillery)
                if (r.count >= 1f && r.moral > 0)
                {
                    int d = m + ((i1 + 1) / 2) * ((i1 & 1) == 0 ? 1 : -1);
                    F[k, d] = r;
                    i1++;
                    if(i1==40)
                    {
                        m = 20;
                        i1 = 0;
                    }
                }
        if (m == 20)
            i0 = i1;
        m = 20;
        foreach (Regiment r in army[k].army)
            if (r.baseRegiment.type == RegimentType.Infantry)
                if (r.count >= 1f && r.moral > 0)
                {
                    int d = m + ((i0 + 1) / 2) * ((i0 & 1) == 0 ? 1 : -1);
                    F[k, d] = r;
                    i0++;
                    if (i0 == 40)
                    {
                        m = 60;
                        i0 = 0;
                    }
                }
        if (m == 60)
            i0 = i1;
        foreach (Regiment r in army[k].army)
            if (r.baseRegiment.type == RegimentType.Cavalry)
                if (r.count >= 1f && r.moral > 0)
                {
                    int d = m + ((i0 + 1) / 2) * ((i0 & 1) == 0 ? 1 : -1);
                    F[k, d] = r;
                    i0++;
                    if (i0 == 40)
                    {
                        m = 60;
                        i0 = i1;
                    }
                }
        for (int i = 40; i < 80; i++)
            if (F[k, i] != null && F[k, i - 40] == null)
            {
                F[k, i - 40] = F[k, i];
                F[k, i] = null;
            }
    }
}
