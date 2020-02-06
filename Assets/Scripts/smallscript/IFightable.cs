using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFightable 
{
    DamageInfo GetDamage(DamageType damageType);

    DamageInfo Hit(DamageInfo damage);
    float AttackRange { get;  set; }
    Vector2Int curPosition { get; }
    State curOwner { get; }
    float LastAttack { get; set; }
    float AttackPeriod { get; set; }
    Vector2 position { get; }
    bool Destoyed { get; set; }
}
