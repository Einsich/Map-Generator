using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class RegimentNode : MonoBehaviour
{
    public string regimentName;
    public int regimentOffset;
    public int Attack, MelleeArmor, ChargeArmor, RangeArmor;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        name = string.Format("{0}_{1} ({2}, {3}, {4}, {5})", regimentName, regimentOffset, Attack, MelleeArmor, ChargeArmor, RangeArmor);

    }
}
