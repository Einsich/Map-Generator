using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {

    public static bool play;
    public static int[,] regionIndex;
    public static List<Region> regions;
    public static List<State> states;
    public static Region curRegion;
    public static Port curPort;
    public static Army army;
    public static Ship ship;
    public static State curPlayer;
    public static Player instance;
    public static Skill CastSkill;
    public static bool CastingSkill => CastSkill != null;
    private void Awake()
    {
        instance = this;
    }
    public void Annexation(State annexator, State target, List<Region> land)
    {
        foreach (var reg in annexator.regions)
            if (reg.ocptby == target)
                reg.ocptby = null;
        foreach(var reg in land)
        {
            reg.owner.regions.Remove(reg);
            reg.owner = annexator;
            reg.ocptby = null;
            annexator.regions.Add(reg);
        }
        foreach (var reg in regions)
            reg.UpdateSplateState(curPlayer);
        annexator.SetName();
        if(target.regions.Count == 0)
        {
            target.DestroyState();
        }
        if (target.stateAI.autoArmyCommander.IsOn)
            target.stateAI.autoArmyCommander.RecalculateRegions();
        if (annexator.stateAI.autoArmyCommander.IsOn)
            annexator.stateAI.autoArmyCommander.RecalculateRegions();
        MapMetrics.UpdateColorMap();
        MapMetrics.UpdateOccupedMap();
        MapMetrics.UpdateSplatMap();
    }
    public void WhitePeace(State annexator, State target)
    {
        foreach (var reg in annexator.regions)
            if (reg.ocptby == target)
                reg.ocptby = null;
        foreach (var army in annexator.army)
            if (army.navAgent.target?.curOwner == target)
                army.navAgent.target = null;
        foreach (var reg in target.regions)
            if (reg.ocptby == annexator)
                reg.ocptby = null;

        foreach (var army in target.army)
            if (army.navAgent.target?.curOwner == annexator)
                army.navAgent.target = null;

        foreach (var reg in regions)
            reg.UpdateSplateState(annexator);
        if (target.stateAI.autoArmyCommander.IsOn)
            target.stateAI.autoArmyCommander.RecalculateRegions();
        if (annexator.stateAI.autoArmyCommander.IsOn)
            annexator.stateAI.autoArmyCommander.RecalculateRegions();
        MapMetrics.UpdateOccupedMap();
        MapMetrics.UpdateSplatMap();
    }
    public void Occupated(Region r, State occupant)
    {
        r.Destoyed = false;
        State realOwner = r.owner;
        State curOwner = r.curOwner;
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

        if (realOwner.stateAI.autoArmyCommander.IsOn)
            realOwner.stateAI.autoArmyCommander.RecalculateRegions();
        
        AutoArmyCommander autoArmy = realOwner.stateAI.autoArmyCommander;
        if (autoArmy.IsOn)
        {
            autoArmy.RecalculateRegions();
        }
        

        if (!r.InFogFrom(curPlayer))
        {
            MapMetrics.SetRegionSplatState(r.territory, LandShowMode.Visible);
        }
        MapMetrics.UpdateOccupedMap();
        MapMetrics.UpdateSplatMap();
    }
    public static void SetState(State state)
    {
        if (state != null)
            MenuManager.SetState(state);

        curPlayer = state;
        foreach (var r in curPlayer.regions)
            if (r.data.portLevel!=0)
                Ship.CreateShip(r);
        foreach (Region r in regions)
        {
            r.UpdateSplateState(curPlayer);
            r.UpdateBorder();
        }
        for (int i = 0; i < states.Count; i++)
        {
            states[i].SetNameStatus(!CameraController.showstate);
            states[i].stateAI.autoBuilder.IsOn = (state != states[i]);
            states[i].stateAI.autoReasercher.IsOn = (state != states[i]);
            states[i].stateAI.autoTrader.IsOn = (state != states[i]);
            states[i].stateAI.autoRegimentBuilder.IsOn = (state != states[i]);
            states[i].stateAI.autoArmyCommander.IsOn = (state != states[i]);
            states[i].stateAI.autoPersonControl.IsOn = (state != states[i]);
            states[i].stateAI.autoDiplomacy.IsOn = (state != states[i]);
        }
        foreach (var army in Army.AllArmy)
        {
            army.GetComponent<ArmyAI>().enabled = army.owner != state;
        }
        foreach (var army in state.army)
        {
            MapMetrics.UpdateAgentVision(army.curPosition, army.curPosition, army.VisionRadius, 1);
        }
        foreach (var ship in state.ships)
        {
            MapMetrics.UpdateAgentVision(ship.curPosition, ship.curPosition, ship.VisionRadius, 1);
        }
        Army.FoggedArmy();
        MapMetrics.UpdateSplatMap();
        MenuManager.HiddenProvinceMenu();
        GameTimer.UpdateListenerQueue();
    }
    public static void SelectArmy(Army sel)
    {
        army = sel;
        army.Selected(true);
        ArmyPanel.Show(true);
        DeselectShip();
    }
    public static void DeselectArmy()
    {
        army?.Selected(false);
        army = null;
        if (CastingSkill)
            StopCastSkill();
        ArmyPanel.Show(false);
    }
    public static void SelectShip(Ship tap)
    {
        ship = tap;
        ship.Select(true);
        curPort = null;
        DeselectArmy();
    }
    public static void DeselectShip()
    {
        ship?.Select(false);
        ship = null;
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
        if (army && army!= tap)
        {
            if (CastingSkill)
            {
                if (CastSkill is IArmyCastable cast)
                {
                    if (cast.CanCastOnArmy(tap) && cast.CastOnArmy(tap))
                        StopCastSkill();
                }
            }
            else
            {
                if (army.owner.diplomacy.haveWar(tap.owner.diplomacy))
                {
                    Region reg = tap.curReg;
                    {
                        army.TryMoveToTarget(tap, TargetType);
                    }
                }
                else
                {
                    army.TryMoveTo(tap.curPosition);
                }
            }
        }
    }
    public static void ShipTap(Ship tap, bool leftClick)
    {
        if (leftClick)
        {
            if (ship != null && ship != tap)
                DeselectShip();
            if (ship == tap)
            {
                DeselectShip();
            }
            else
            if (curPlayer == null || tap.curOwner == curPlayer)
            {
                SelectShip(tap);
            }
        } else
        {
            if(army && tap.CanSetOn(army))
            {
                tap.SetOnArmy(army);
                DeselectArmy();
                SelectShip(tap);
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
                if (CastingSkill)
                {
                    if (CastSkill is IRegionCastable cast)
                    {
                            if (cast.CanCastOnRegion(tap) && cast.CastOnRegion(tap))
                            {
                                StopCastSkill();
                            }
                        
                    }
                    else
                    {
                        StopCastSkill();
                    }
                }
                else
                {
                    tap.Selected = true;
                    curRegion = tap;
                    MenuManager.ShowProvinceMenu(curRegion);
                }
            }
        }
        else if (curRegion != null)
        {
            curRegion.Selected = false;
            curRegion = null;
        }
    }
    public static void StartCastSkill(Skill skill)
    {
        CastSkill = skill;
        InputManager.Instance.SpaceAction += StopCastSkill;
        cursorType =  CursorType.CastSkill;
        CursorHandler.SetCursor(cursorType);
    }
    public static void StopCastSkill()
    {
        CastSkill = null;
        InputManager.Instance.SpaceAction -= StopCastSkill;
    }
    static CursorType cursorType = CursorType.Default;
    private void Start()
    {
        CursorHandler.SetCursor(cursorType);
    }
    void Update() {
        CursorType cursor = CursorType.Default;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (CastingSkill)
                cursor = CursorType.CastSkill;
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

                    if (Input.GetMouseButtonDown(1))
                    {
                        army?.TryMoveTo(hit.point);
                        ship?.TryMoveTo(hit.point);
                        if(curPort)
                        {
                            curPort.ShipOut(hit.point);
                        }
                    }
                }
                State other = null;
                IFightable target = null;
                if (hit.transform.tag == "Unit")
                {
                    Army sel = hit.transform.GetComponent<Army>();
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    {
                        ArmyTap(sel, Input.GetMouseButtonDown(0));
                    }
                    other = sel.curOwner;
                    target = sel;
                    if (CastingSkill)
                    {
                        if (CastSkill is IArmyCastable cast)
                        {
                            if (cast.CanCastOnArmy(sel))
                                cursor = CursorType.CanCastSkill;                            
                        }
                    }
                    else
                    {
                        if (other == curPlayer && sel != army)
                            cursor = CursorType.Select;
                    }                   
                }
                if (hit.transform.tag == "Ship")
                {
                    Ship sel = hit.transform.GetComponent<Ship>();
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    {
                        ShipTap(sel, Input.GetMouseButtonDown(0));
                    }
                    other = sel.curOwner;
                    if (CastingSkill)
                    {
                    }
                    else
                    {
                        if (other == curPlayer && sel != ship)
                            cursor = CursorType.Select;
                    }
                }
                if (hit.transform.tag == "Town")
                {
                    Region reg = MapMetrics.regions[int.Parse(hit.transform.name)];
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    {
                        if (Input.GetMouseButtonDown(0))
                            RegionTap(reg);
                        if (army && Input.GetMouseButtonDown(1))
                        {

                            if (reg.curOwner == army.owner)
                                army.TryMoveTo(reg.Capital);
                            else
                            {
                                if (army.besiege != reg && TargetType == DamageType.Melee)
                                    army.TryMoveToTarget(reg, TargetType);
                            }
                        }
                    }
                    other = reg.curOwner;
                    target = reg;
                    if (CastingSkill)
                    {
                        if(CastSkill is IRegionCastable cast)
                        {
                            if (cast.CanCastOnRegion(reg))
                                cursor = CursorType.CanCastSkill;
                            
                        }
                    }
                    else
                    {
                        if (other == curPlayer && reg != curRegion)
                            cursor = CursorType.Select;
                    }
                }
                if (hit.transform.GetComponent<Port>())
                {
                    Port port = hit.transform.GetComponent<Port>();
                    if(Input.GetMouseButtonDown(0) && port.curOwner == curPlayer && port.Ship)
                    {
                        curPort = port;
                        DeselectShip();
                    }
                    if(Input.GetMouseButtonDown(1) && ship && port.CanshipOn(ship))
                    {
                        ship.TryMoveTo(port);
                    }

                }
                if (!CastingSkill && target != null && army != null &&army.CanAttack(target, TargetType))
                {
                    switch (TargetType)
                    {
                        case DamageType.Melee:
                        case DamageType.Charge: cursor = CursorType.MeleeAttack; break;
                        case DamageType.Range: cursor = CursorType.RangeAttack; break;
                        case DamageType.Siege: cursor = CursorType.SiegeAttack; break;
                    }
                }
            }
        }
        if (curRegion == null)
        {
            MenuManager.HiddenProvinceMenu();
        }
        if(cursor != cursorType)
        {
            cursorType = cursor;
            CursorHandler.SetCursor(cursorType);
        }
    }

    static DamageType TargetType { get { DamageType target = DamageType.Melee;
            if (Input.GetKey(KeyCode.LeftAlt))
                target = DamageType.Range;
            if (Input.GetKey(KeyCode.LeftControl))
                target = DamageType.Siege;
            return target; } }
}
