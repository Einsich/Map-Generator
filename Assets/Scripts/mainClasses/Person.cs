using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person
{
    public Sprite icon => SpriteHandler.GetPersonIcon(personType);
    static string[] names = {  "Yan", "Yanis", "Yanka", "Yanchik", "Yanushka", "Yandex" };
    ///<summary>pips [fire, shock, smt]</summary>
    public int[] pips = new int[3];
    public string name;
    public PersonType personType;
    public bool die, inTavern = true;
    public PersonAliveAction alive = null;
    public Army curArmy;
    public State owner;

    public void Die()
    {
        (this as Trader)?.UpdateInfluence(curArmy.curReg, false);
        die = true;
        inTavern = true;
    }
    public void Alive()
    {
        die = false;
        alive = null;
        //curArmy
    }
    public void SetToCapital()
    {
        if (curArmy == null)
            curArmy = Army.CreateArmy(owner.Capital, owner.defaultArmy(), this);
        else
            curArmy.InitArmy(new List<Regiment>(), owner.Capital, this);
        inTavern = false;
    }
    public Person(State state)
    {
        owner = state;
        owner.persons.Add(this);
        name = names[Random.Range(0, names.Length)];
        die = Random.Range(0, 2) == 1;
    }
    virtual public void NewLevel() { lvlPoint++; }

    int exp_ = 0;
    public int lvlPoint=0;
    public int exp
    {
        get => exp_; set
        {
            exp_ = value;
            while (nextLvl <= exp_)
            {
                exp_ -= nextLvl;
                lvl++;
                NewLevel();
            }
        }
    }
    public int lvl { get; private set; } = 1;
    public int nextLvl => lvl * lvl * 100;
    public float expf => 1f * exp_ / nextLvl;
    
}
public class Knight:Person
{
    override public void NewLevel()
    {
        base.NewLevel();
        pips[Random.Range(0, 3)]++;        
    }
    public Knight(State state):base (state)
    {
        for (int i = 0; i < 3; i++)
            pips[i] = Random.Range(1, 3);
        personType = PersonType.Knight;
    }
}
public class Trader : Person
{
    public float traderEconomyImpact => lvl * 0.2f + 1f;
   // int intImpact => lvl * 20 + 100;
    override public void NewLevel()
    {
        base.NewLevel();
    }
    public Trader(State state) : base(state)
    {
        personType = PersonType.Trader;
    }

    public void UpdateInfluence(Region cur, bool add)
    {
        float impact = add ? traderEconomyImpact : 1 / traderEconomyImpact;
        if (cur != null)
        {
            if (cur.curOwner == owner)
                cur.data.traderImpact *= impact;
            foreach (var neib in cur.neib)
                if (neib.curOwner == owner)
                    neib.data.traderImpact *= impact;
        }
    }
}
public enum PersonType
{
    Knight,
    Trader
}
