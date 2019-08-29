using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person
{
    public static Sprite[] icons;
    public Sprite icon => icons[(int)personType];
    static string[] names = { "No name", "Yan", "Yanis", "Yanka", "Yanchik", "Yanushka", "Yandex" };
    ///<summary>pips [fire, shock, smt]</summary>
    public int[] pips = new int[3];
    public string name;
    public PersonType personType;
    public bool die;
    public PersonAliveAction alive = null;
    public void Die()
    {
        die = true;
    }
    public void Alive()
    {
        die = false;
        alive = null;
    }
    public Person()
    {
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
    public Knight()
    {
        for (int i = 0; i < 3; i++)
            pips[i] = Random.Range(1, 3);
        personType = PersonType.Knight;
    }
}
public class Trader : Person
{
    override public void NewLevel()
    {
        base.NewLevel();
    }
    public Trader()
    {
        personType = PersonType.Trader;
    }
}
public enum PersonType
{
    Knight,
    Trader
}
