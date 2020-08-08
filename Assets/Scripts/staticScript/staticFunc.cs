using UnityEngine;
using System.Collections.Generic;



public static partial class staticFunc
{

    public static string ToPercent(this float f)
    {
        return Mathf.Abs((f - 1) * 100).ToString("N2") + " %";
    }

}
  
