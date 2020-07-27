using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockHero", menuName = "Technology/UnlockHero", order = 1)]
public class UnlockHeroTechnology : Technology
{
    public PersonType personType;
    public override void LevelUp() => tree.openedPerson.Add(personType);
}