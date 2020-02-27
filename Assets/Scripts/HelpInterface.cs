using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHelpBaseRegiment
{
    BaseRegiment BaseRegiment { get;  set; }

}
public interface IHelpTechology
{
    Tech Technology { get; set; }

}
public interface IHelpPerson
{
    Person Person { get; set; }
}
public interface IHelpBuilding
{
    string BuildingDescribe();
}