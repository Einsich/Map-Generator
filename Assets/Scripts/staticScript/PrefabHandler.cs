using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PrefabHandler 
{
    static GameObject[] ArmyPrefab;
    static PrefabData PrefabData;
    static PrefabHandler()
    {

        PrefabData = Resources.Load<PrefabData>("PrefabData");
        ArmyPrefab = PrefabData.Persons;
    }
    public static GameObject GetArmyPrefab(PersonType type)
    {
        GameObject prefab = null;
        switch(type)
        {
            case PersonType.Warrior:
            case PersonType.Knight:prefab = ArmyPrefab[0];break;
            case PersonType.Jaeger:
            case PersonType.Archer: prefab = ArmyPrefab[2]; break;

            case PersonType.Wizard:
            case PersonType.Engineer: prefab = ArmyPrefab[3]; break;

            case PersonType.Necromancer:prefab = ArmyPrefab[1];break;
            case PersonType.DeathKnight: prefab = ArmyPrefab[4]; break;
        }
        return prefab;
    }
    public static GameObject GetSiegePrefab => PrefabData.SiegeBrefab;
    public static ArmyBar GethpBarPrefab => PrefabData.HPBarBrefab;
    public static TownBar GethpTownBarPrefab => PrefabData.HPTownBarBrefab;
    public static Ship GetShip => PrefabData.ShipPrefab;
    public static Port GetPort(FractionType fraction) => PrefabData.Ports[(int)fraction];
    public static BaseRegiment GetSkeleton(int lvl) => PrefabData.Skelets[lvl];
    public static int SkeletCount => PrefabData.Skelets.Length;

    public static TechInterface TechnologyPanelPrefab => PrefabData.TechnologyPanel;
    public static RegimentTeches RegimentTechnologyPanelPrefab => PrefabData.RegimentTechnologyPanel;
    public static TechnologyTree TechnologyTree => PrefabData.TechnologyTree;
}
