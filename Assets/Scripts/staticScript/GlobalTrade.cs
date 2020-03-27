using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalTrade 
{
    static Treasury Income = new Treasury(1);
    public static void DiscardCource() => Income = new Treasury();
    public static void AddIncome(Treasury income) => Income += income;

    public static float GetCource(ResourcesType bye, ResourcesType sell) => Mathf.Min(Income[(int)bye] / Income[(int)sell], 1000);

}
