using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipBuildPanel : MonoBehaviour
{
    public Button buildShip;
    Port port;
    public void Open(Port port)
    {
        this.port = port;
        port.ShipChange += UpdateButton;
        port.Region.data.SomeChanges += UpdateButton;
        buildShip.onClick.RemoveAllListeners();
        buildShip.onClick.AddListener(() => port.ShipOn(Ship.CreateShip(port.Region)));
        buildShip.onClick.AddListener(UpdateButton);
        UpdateButton();
    }
    private void OnDisable()
    {
        port.ShipChange -= UpdateButton;
        port.Region.data.SomeChanges -= UpdateButton;

    }
    private void UpdateButton()
    {
        {
            buildShip.interactable = port.Ship == null && port.Region.data.portLevel > 0;
        } 
    }
}
