using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IHelpBuilding
{

    //состояния 0-недоступно, 1-можно строить, 2-строится
    //ничего нельзя поделать, можно построить, прекратить постройку
    Image image,stateImage;
    Button button;
    public int BuildIndex;
    private ProvinceData data;
    private GameAction CurBuildAction;
    private BuildState state;
    void Awake()
    {
        image = GetComponent<Image>();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(PressButton);
        stateImage = button.GetComponent<Image>();
    }
    void Update () {
        if (button.interactable)
        {
            float s = Mathf.Cos(Time.time * 3);
            stateImage.color = Color.Lerp(Color.white, dark, s * s);
            if(state == BuildState.isBuilding && CurBuildAction != null)
                image.fillAmount = CurBuildAction.progress;
        }
        
	}
    static Color trans = new Color(0, 0, 0, 0),
        dark = new Color(1, 1, 1, 0.4f);
    public void OnEnable()
    {
        if (Player.curRegion == null)
            return;
        data = Player.curRegion.data;
        
        image.sprite = SpriteHandler.GetBuilding(BuildIndex, (int)Player.curPlayer.fraction);

        UpdateState();
        Player.curPlayer.TreasureChange += UpdateState;
        CurBuildAction = data.BuildingAction[BuildIndex];
        if (CurBuildAction != null)
            CurBuildAction.onAction += UpdateState;
    }
    private void OnDisable()
    {
        if (Player.curRegion == null)
            return;
        Player.curPlayer.TreasureChange -= UpdateState;
        if (CurBuildAction != null)
            CurBuildAction.onAction -= UpdateState;
    }
    private void UpdateState()
    {

        button.GetComponentInChildren<Text>().text = (data.buildings[BuildIndex]).ToString();
        state = data.Stateof((BuildingType)BuildIndex);

        if (state != BuildState.isBuilding)
            image.fillAmount = 1f;
        if (state == BuildState.CantBuild)
        {
            button.interactable = false;
            stateImage.sprite = null;
            image.color = Color.gray;
            stateImage.color = trans;
        }
        else
        {
            button.interactable = true;
            image.color = Color.white;
            stateImage.sprite = SpriteHandler.GetBuildFrame(state);            
        }
    }
    public void PressButton()
    {
        data.ClickBuilding(BuildIndex);
        CurBuildAction = data.BuildingAction[BuildIndex];
        UpdateState();
    }

    public string BuildingDescribe()
    {
        return data.BuildingDescription((BuildingType)BuildIndex);
        
    }
}
