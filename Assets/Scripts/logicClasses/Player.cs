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
    public void Occupated(Region r,State occupant)
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
        if(state!=null)
        MenuManager.SetState(state);

        curPlayer = state;
        foreach (Region r in regions)
        {
            r.UpdateSplateState(curPlayer);
            r.UpdateBorder();
        }
        for(int i=0;i<states.Count;i++)
        {
            states[i].SetNameStatus(!CameraController.showstate);                
        }
        Army.UpdateUnitFog();
        MapMetrics.UpdateSplatMap();
        MenuManager.HiddenProvinceMenu();
    }
    void Update () {
        if (!EventSystem.current.IsPointerOverGameObject()&&(Input.GetMouseButtonDown(0)|| Input.GetMouseButtonDown(1)))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Map")
                {
                    int x = (int)hit.point.x;
                    int y = (int)hit.point.z;
                    if(Input.GetMouseButtonDown(0))
                    {
                        if (!MapMetrics.InsideMap(y, x))
                        {
                            if (curRegion != null)
                                curRegion.Selected = false;
                            curRegion = null;
                        }
                        else
                        {
                            int i = regionIndex[y, x];


                            if (curRegion != regions[i] && !regions[i].HiddenFrom(curPlayer))
                            {
                                if (curRegion != null)
                                    curRegion.Selected = false;
                                if (regions[i].iswater)
                                {
                                    if (curRegion != null)
                                    {
                                        curRegion.Selected = false;
                                        curRegion = null;
                                    }
                                }
                                else
                                {
                                    regions[i].Selected = true;
                                    curRegion = regions[i];
                                    MenuManager.ShowProvinceMenu(curRegion);
                                }
                            }
                            else
                            if (curRegion != null)
                            {
                                curRegion.Selected = false;
                                curRegion = null;
                            }
                        }
                    }

                    if (Input.GetMouseButtonDown(1) && army && army.action != ArmyAction.Retreat)
                    {
                        if (army.curBattle == null)
                            army.TryMoveTo(hit.point);
                        else
                            army.curBattle.EndBattle(army.owner);
                    }
                }
                else if (curRegion != null)
                {
                    curRegion.Selected = false;
                    curRegion = null;
                }
                if (hit.transform.tag == "Unit")
                {
                        Army sel = hit.transform.GetComponent<Army>();
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (army != null && army != sel)
                        {
                            army.Selected(false);
                            army = null;
                            ArmyPanel.Show(false);
                        }
                        if (army == sel)
                        {
                            army.Selected(false);
                            army = null;
                            ArmyPanel.Show(false);
                        }
                        else
                        if (curPlayer == null || sel.owner == curPlayer)
                        {

                            army = sel;
                            army.Selected(true);
                            ArmyPanel.Show(true);
                        }
                    }
                    else
                        if (army && Input.GetMouseButtonDown(1))
                    {
                        army.TryMoveTo(hit.point);
                    }
                    else
                         if (army != null)
                    {
                        army.Selected(false);
                        army = null;
                            ArmyPanel.Show(false);
                    }
                }
            }
        }
        if(curRegion==null)
        {
            MenuManager.HiddenProvinceMenu();
        }
    }


}
