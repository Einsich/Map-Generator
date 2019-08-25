using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Regiment
{

    public float moral, maxmoral;
    public int count, loses;
    public BaseRegiment baseRegiment;
    public float moralF => moral / maxmoral;
    public float countF => count / 1000f;

    public Regiment()
    {

    }
    public Regiment(BaseRegiment t)
    {
        count = 1000;
        moral = maxmoral = 2f;
        baseRegiment = t;
    }
}
public class General
{
    ///<summary>pips [fire, shock, smt]</summary>
    public int[] pips = new int[3];
    public string name;
    public General()
    {
        name = "Нет генерала";
    }
}
public class BaseRegiment
{
    ///<summary>pips [fire, shock, moral; attack, defence]</summary>
    public int[,] pips;
    public RegimentType type;
    public RegimentName name;
    public int time;
    public Treasury cost, upkeep;
    public Sprite Icon => BattleInterface.allRegimentType[(int)type];

}
public enum RegimentName
{
    Пехота,
    Кавалерия,
    Пушки
}
public enum RegimentType
{
    Infantry,
    Cavalry,
    Artillery
}
