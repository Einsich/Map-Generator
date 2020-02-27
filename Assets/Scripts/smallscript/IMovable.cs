using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovable
{

    State curOwner { get; }
    void CellChange(Vector2Int oldCell, Vector2Int newCell);
    void Stop();
    float Speed { get; set; }

}
