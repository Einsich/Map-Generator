using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabDataBase", menuName = "PrefabDataBase", order = 51)]
public class PrefabData : ScriptableObject
{
    [SerializeField] public GameObject[] Persons;
    [SerializeField] public GameObject SiegeBrefab;
    [SerializeField] public ArmyBar HPBarBrefab;
    [SerializeField] public Port[] Ports;
    [SerializeField] public Ship ShipPrefab;
    [SerializeField] public BaseRegiment[] Skelets;
    [SerializeField] public TechInterface TechnologyPanel;
    [SerializeField] public RegimentTeches RegimentTechnologyPanel;
    [SerializeField] public TechnologyTree TechnologyTree;
    [SerializeField] public TownBar HPTownBarBrefab;
}
