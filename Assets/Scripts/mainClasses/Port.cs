using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Port : MonoBehaviour
{
    public Vector2 CornerPosition;
    public Ship Ship { get; private set; }
    public Region Region;
    public State curOwner => Region.owner;
    public System.Action ShipChange;
    public void ShipOn(Ship ship)
    {
        ship.gameObject.SetActive(false);
        Ship = ship;
        ship.Port = this;
        ShipChange?.Invoke();
    }
    public void ShipOut(Vector3 point)
    {
        if(Ship.TryMoveTo(point))
        {
            Ship.SetPosition(CornerPosition);
            Ship.gameObject.SetActive(true);
            if (Player.curPort == this)
            {
                Player.SelectShip(Ship);
                Player.curPort = null;
            }
            Ship = null;
            ShipChange?.Invoke();

        }
    }
    public bool CanshipOn(Ship ship)
    {
        return Ship == null && ship.curOwner == Region.owner;
    }
}
