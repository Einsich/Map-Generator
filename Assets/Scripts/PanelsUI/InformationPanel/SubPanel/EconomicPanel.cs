using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EconomicPanel : MonoBehaviour
{
    [SerializeField] private Slider armyBudget, buildBudget, techBudget, otherBudget;
    [SerializeField] private Text armyBudgetText, buildBudgetText, techBudgetText, otherBudgetText, allBudgetText;
    [SerializeField] private Toggle autoArmy, autoBuild, autoResearch;

    private State state;

    public void ShowEconomic(State state)
    {
        this.state = state;
        state.IncomeChanges += UpdateInformation;
        state.TreasureChange += UpdateInformation;
        armyBudget.onValueChanged.AddListener((x)=> SliderValueChange(x, BudgetType.ArmyBudget));
        buildBudget.onValueChanged.AddListener((x) => SliderValueChange(x, BudgetType.BuildingBudget));
        techBudget.onValueChanged.AddListener((x) => SliderValueChange(x, BudgetType.TechnologyBudget));

        autoArmy.onValueChanged.AddListener((x) => state.stateAI.autoRegimentBuilder.IsOn = x);
        autoBuild.onValueChanged.AddListener((x)=> state.stateAI.autoBuilder.IsOn = x);
        autoResearch.onValueChanged.AddListener((x)=>state.stateAI.autoReasercher.IsOn = x);
        SliderUpdate();
        ToggleUpdate();
        UpdateInformation();
    }
    private void ToggleUpdate()
    {
        autoArmy.isOn = state.stateAI.autoRegimentBuilder.IsOn;
        autoBuild.isOn = state.stateAI.autoBuilder.IsOn;
        autoResearch.isOn = state.stateAI.autoReasercher.IsOn; 
    }
    private void SliderUpdate()
    {
        armyBudget.value = state.stateAI.armyBudget;
        buildBudget.value = state.stateAI.buildingBudget;
        techBudget.value = state.stateAI.technologyBudget;
        otherBudget.value = 1 - state.stateAI.armyBudget - state.stateAI.buildingBudget;
    }
    private void UpdateInformation()
    {
        //use it
        //state.regions[i].data.CalculateIncome(); - Доход провинции 
        //state.army[i].GetUpkeep();
        StateAI ai = state.stateAI;
        armyBudgetText.text = ai.GetArmyBudget.ToString();
        buildBudgetText.text = ai.GetBuildingBudget.ToString();
        techBudgetText.text = ai.GetTechnologyBudget.ToString();
        otherBudgetText.text = ai.GetOtherBudget.ToString();
        allBudgetText.text = ai.GetTreasure.ToString();
    }
    void SliderValueChange(float t, BudgetType budgetType)
    {
        state.stateAI.ChangeBudget(t, budgetType);
        SliderUpdate();
        UpdateInformation();
    }
    private void OnDisable()
    {
        if (state == null)
            return;
        state.IncomeChanges -= UpdateInformation;
        state.TreasureChange -= UpdateInformation;
        armyBudget.onValueChanged.RemoveAllListeners();
        buildBudget.onValueChanged.RemoveAllListeners();
        techBudget.onValueChanged.RemoveAllListeners();

        autoArmy.onValueChanged.RemoveAllListeners();
        autoBuild.onValueChanged.RemoveAllListeners();
        autoResearch.onValueChanged.RemoveAllListeners();
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

