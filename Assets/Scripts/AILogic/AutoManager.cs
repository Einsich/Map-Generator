using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AutoManager 
{
     bool IsOn { get;  set; }
}
public interface AutoSpender
{
    bool IsOn { get; set; }
    AutoSpenderResult TrySpend(StateAI state);
}
public enum AutoSpenderResult
{
    Success,
    NeedMoreResources,
    HasNotOrder
}