using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Start () {
        selected = null;
	}
    public static bool play;
    public static int[,] prov;
    public static List<Region> reg;
    public static List<State> states;
    public static Region selected;
    public static Army army;
    public static State curPlayer;
    public static Player that;
    private void Awake()
    {
        that = this;
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
        MainStatistic.that.SetState(state);

        curPlayer = state;
        foreach (Region r in reg)
        {
            r.UpdateSplateState(curPlayer);
            r.UpdateBorder();
        }
        for(int i=1;i<states.Count;i++)
        {
            states[i].SetNameStatus(!CameraController.showstate);                
        }
        Army.UpdateUnitFog();
        MapMetrics.UpdateSplatMap();
        ProvinceMenu.that.HiddenProvinceMenu();
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
                            if (selected != null)
                                selected.Selected = false;
                            selected = null;
                        }
                        else
                        {
                            int i = prov[y, x];


                            if (selected != reg[i] && !reg[i].HiddenFrom(curPlayer))
                            {
                                if (selected != null)
                                    selected.Selected = false;
                                if (reg[i].iswater)
                                {
                                    if (selected != null)
                                    {
                                        selected.Selected = false;
                                        selected = null;
                                    }
                                }
                                else
                                {
                                    reg[i].Selected = true;
                                    selected = reg[i];
                                    ProvinceMenu.that.ShowProvinceMenu(selected);
                                }
                            }
                            else
                            if (selected != null)
                            {
                                selected.Selected = false;
                                selected = null;
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
                else if (selected != null)
                {
                    selected.Selected = false;
                    selected = null;
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
                        }
                        if (army == sel)
                        {
                            army.Selected(false);
                            army = null;
                        }
                        else
                        if (curPlayer == null || sel.owner == curPlayer)
                        {

                            army = sel;
                            army.Selected(true);
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
                    }
                }
            }
        }
        if(selected==null)
        {
            ProvinceMenu.that.HiddenProvinceMenu();
        }
    }


}
