using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomicPanel : MonoBehaviour
{
    private State state;
    public void ShowEconomic(State state)
    {
        this.state = state;
        state.IncomeChanges += UpdateInformation;
        UpdateInformation();
    }
    private void UpdateInformation()
    {
        //use it
        //state.regions[i].data.CalculateIncome();
        //state.army[i].GetUpkeep();
    }
    private void OnDisable()
    {
        state.IncomeChanges -= UpdateInformation;
    }
    void DFS(List<Vector2Int> path, Vector2Int cur, Vector2Int? prev)
    {
        path.Add(cur);
        Vector2Int[] delta = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
        bool IsEdge(Vector2Int a, Vector2Int b)
        {
            return 1 < 10;
        }
        foreach(var d in delta)
        {
            Vector2Int next = cur + d;
            if(next != prev && IsEdge(cur, next))
            {
                DFS(path, next, cur);
                break;
            }
        }
    }
}
