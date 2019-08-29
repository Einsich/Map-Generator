using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    ITarget target;
    Vector2Int curCell;
    Vector2 pos;
    State owner;
    Army army;
    public void SetToArmy(Army army)
    {
        this.army = army;
        pos = army.pos;
        owner = army.owner;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    static Vector2Int ToInt(Vector2 p) => new Vector2Int((int)p.x, (int)p.y);
    public bool MoveTo(Vector2 to)
    {
        return false;
        List<Vector2Int> path = Main.FindPath(ToInt(pos), ToInt(to), owner);

    }
}
