﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {

    public static bool play;
    public static int[,] regionIndex;
    public static List<Region> regions;
    public static List<State> states;
    public static Region curRegion;
    public static Army army;
    public static State curPlayer;
    public static Player instance;
    private void Awake()
    {
        instance = this;
        curPlayer = null;
        curRegion = null;
        army = null;
    }
    public void Occupated(Region r, State occupant)
    {

        if (occupant != r.owner)
        {
            r.ocptby = occupant;

            Debug.Log(string.Format("Страна {0} захватила {1}", occupant.name, r.name));
        }
        else
        {
            r.ocptby = null;

            Debug.Log(string.Format("Страна {0} вернула {1}", occupant.name, r.name));
        }

        if (!r.InFogFrom(occupant))
        {
            MapMetrics.SetRegionState(r.territory, LandShowMode.Visible);
        }
        MapMetrics.UpdateOccupedMap();
        MapMetrics.UpdateSplatMap();
    }
    public static void SetState(State state)
    {
        if (state != null)
            MenuManager.SetState(state);

        curPlayer = state;
        foreach (Region r in regions)
        {
            r.UpdateSplateState(curPlayer);
            r.UpdateBorder();
        }
        for (int i = 0; i < states.Count; i++)
        {
            states[i].SetNameStatus(!CameraController.showstate);
        }
        foreach (var army in Army.AllArmy)
            army.GetComponent<ArmyAI>().enabled = army.owner != state;
        Army.FoggedArmy();
        MapMetrics.UpdateSplatMap();
        MenuManager.HiddenProvinceMenu();
    }
    public static void SelectArmy(Army sel)
    {
        army = sel;
        army.Selected(true);
        ArmyPanel.Show(true);
    }
    public static void DeselectArmy()
    {
        army.Selected(false);
        army = null;
        ArmyPanel.Show(false);
    }
    public static void ArmyTap(Army tap, bool leftClick)
    {
        if (leftClick)
        {
            if (army != null && army != tap)
                DeselectArmy();
            if (army == tap)
                DeselectArmy();
            else
            if (curPlayer == null || tap.owner == curPlayer)
                SelectArmy(tap);
        }
        else
        if (army)
        {
            if (army.owner.diplomacy.haveWar(tap.owner.diplomacy))
            {
                Region reg = tap.curReg;
                if (army.besiege != reg || !tap.inTown)
                {
                    army.TryMoveToTarget(tap, TargetType);
                }
            } else
            {
                army.TryMoveTo(tap.curPosition);
            }
        }
    }
    public static void RegionTap(Region tap)
    {
        if (curRegion != tap && !tap.HiddenFrom(curPlayer))
        {
            if (curRegion != null)
                curRegion.Selected = false;
            if (tap.iswater)
            {
                if (curRegion != null)
                {
                    curRegion.Selected = false;
                    curRegion = null;
                }
                if (army != null)
                    DeselectArmy();
            }
            else
            {
                tap.Selected = true;
                curRegion = tap;
                MenuManager.ShowProvinceMenu(curRegion);
            }
        }
        else if (curRegion != null)
        {
            curRegion.Selected = false;
            curRegion = null;
        }
    }
    void Update() {
        if (!EventSystem.current.IsPointerOverGameObject() && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                int x = (int)hit.point.x;
                int y = (int)hit.point.z;
                if (hit.transform.tag == "Map")
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!MapMetrics.InsideMap(y, x))
                        {
                            if (curRegion != null)
                                curRegion.Selected = false;
                            curRegion = null;
                            if (army != null)
                                DeselectArmy();
                        }
                        else
                        {
                            RegionTap(Main.regions[regionIndex[y, x]]);
                        }
                    }

                    if (Input.GetMouseButtonDown(1) && army)
                    {
                        army.TryMoveTo(hit.point);
                    }
                }
                if (hit.transform.tag == "Unit")
                {
                    Army sel = hit.transform.GetComponent<Army>();
                    ArmyTap(sel, Input.GetMouseButtonDown(0));
                }
                if (hit.transform.tag == "Town")
                {
                    Region reg = MapMetrics.regions[int.Parse(hit.transform.name)];
                    if (Input.GetMouseButtonDown(0))
                        RegionTap(reg);
                    if (army && Input.GetMouseButtonDown(1))
                    {

                        if (reg.curOwner == army.owner)
                            army.TryMoveTo(reg.Capital);
                        else
                        {
                            if (army.besiege != reg)
                                army.TryMoveToTarget(reg, TargetType);
                        }
                    }
                }
            }
        }
        if (curRegion == null)
        {
            MenuManager.HiddenProvinceMenu();
        }
    }

    static DamageType TargetType { get { DamageType target = DamageType.Melee;
            if (Input.GetKey(KeyCode.LeftAlt))
                target = DamageType.Range;
            if (Input.GetKey(KeyCode.LeftControl))
                target = DamageType.Siege;
            return target; } }
}
