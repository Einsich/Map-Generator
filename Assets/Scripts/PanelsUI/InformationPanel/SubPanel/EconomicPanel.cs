using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        //state.regions[i].data.CalculateIncome(); - Доход провинции 
        //state.army[i].GetUpkeep();
    }
    private void OnDisable()
    {
        state.IncomeChanges -= UpdateInformation;
    }
    
}

class Block:MonoBehaviour
{
    [SerializeField] private int[] Color = new int[4];
    public static void Generate(Vector2Int pos, Block[] prefabs)
    {
        Treasury GlobalIncome= default;//(gold =  100, wood = 10)
        float Gold = 177;
        float Wood = Gold * GlobalIncome.Wood / GlobalIncome.Gold;//Gold * 10/ 100 = Gold * 0.1;
        /*int n = 100, k = 0;
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector3Int, Block> dictionary = new Dictionary<Vector3Int, Block>();
        Vector2Int[] dxy = new Vector2Int[4] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        
        q.Enqueue(pos);
        while (q.Count > 0 && k++ < n)
        {
            Vector2Int p = q.Dequeue();
            int[] conditional = new int[4]; 
            for (int i = 0; i < dxy.Length; i++)
            {
                Vector2Int t = p + dxy[i];
                Block block;
                if(dictionary.TryGetValue(t, out block))
                {
                    conditional[i] = block.Color[i];
                }
            }
           // Block prefab = GetBlock(conditional, prefabs);
            //Instantiate(prefab, new Vector3(p.x, p.y, 0));
        }
        */
    }
}

