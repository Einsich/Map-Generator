using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITarget
{
   // Vector2Int curPosition { get; }
   // State curOwner { get; }
    //void WasCatch(Army catcher);
}
public enum TargetType
{
    MeleeAttack,
    RangeAttack,
    SiegeAttack
}