using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class TechnologyNode : MonoBehaviour
{
    public TechnologySystem.Technology technology;
    public static int nodeIDCounter = 0;
    private int nodeID = -1;
    public int getID => nodeID == -1 ? nodeID = nodeIDCounter++ : nodeID;

    // Update is called once per frame
    void Update()
    {
        if (technology != null)
            name = technology.name;
    }
}

