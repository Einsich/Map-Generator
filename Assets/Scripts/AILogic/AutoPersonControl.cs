using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPersonControl : AutoManager
{
    private StateAI state;
    public AutoPersonControl(StateAI stateAI)
    {
        state = stateAI;
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
                PersonUpdate();
            }
            else
            {

            }
            isOn = value;
        }
    }

    public Treasury NeedTreasure => throw new System.NotImplementedException();

    public void PersonUpdate()
    {
        if (!isOn)
            return;
        foreach (var person in state.Data.persons)
            if (!person.die)
            {
                if(person.lvlPoint > 0)
                {
                    if(Random.value > 0.3f)
                    {
                        person.LearnProperty((PropertyType)Random.Range(0, 5));
                    } else
                    {
                        int i = Random.Range(0, person.Skills.Length);
                        if (person.Skills[i].GetLevel != person.Skills[i].GetMaxLevel)
                            person.LearnSkill(i);
                    }
                }
                foreach(var skill in person.Skills)
                {
                    if (skill.GetLevel == 0)
                        continue;
                    if(skill is IArmyCastable armyCastable )
                    {

                    } else 
                    if(skill is IRegionCastable regionCastable)
                    {

                    } else
                    {
                        if (skill is SacrificeSkill)
                            continue;
                        skill.Cast();
                    }
                }
            }
    }
}
