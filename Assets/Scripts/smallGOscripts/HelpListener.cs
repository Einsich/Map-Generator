using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HelpListener : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField]private HelpPanelDirection direction;
    [SerializeField]private HelpPanelSize size;
    [SerializeField]private HelpShowClass advance;
    [SerializeField]private string Key;
    private bool HelpElement;
    private IHelpBaseRegiment IHelpBaseRegiment;
    private IHelpTechology IHelpTechology;
    private IHelpPerson IHelpPerson;
    private IHelpBuilding IHelpBuilding;
    private float LastAction = 0;
    private bool Inside = false, Helping = false;
    static HelpListener LastHelper = null;
    private void Start()
    {
        switch (advance)
        {
            case HelpShowClass.BaseRegiment:
                IHelpBaseRegiment = GetComponentInParent<RecruitListElement>();
                if (IHelpBaseRegiment == null)
                    IHelpBaseRegiment = GetComponentInParent<RecruitQueueElement>();
                if (IHelpBaseRegiment == null)
                    IHelpBaseRegiment = GetComponentInParent<GarnisonIcon>();
                if (IHelpBaseRegiment == null)
                    IHelpBaseRegiment = GetComponentInParent<RegimentTeches>();
                HelpElement = IHelpBaseRegiment != null;
                break;
            case HelpShowClass.Technology:
                IHelpTechology = GetComponentInParent<TechInterface>();
                HelpElement = IHelpTechology != null;

                break;
            case HelpShowClass.Person:
                IHelpPerson = GetComponentInParent<ArmyPanel>();
                if (IHelpPerson == null)
                    IHelpPerson = GetComponentInParent<PersonInformation>();
                HelpElement = IHelpPerson != null;

                break;
            case HelpShowClass.Building:
                IHelpBuilding = GetComponentInParent<BuildingButton>();
                HelpElement = IHelpBuilding != null;

                break;

        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        LastAction = Time.time;
        Inside = true;
    }
    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        if (LastHelper == this)
            Hide();

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        LastAction = Time.time;
        Inside = false;
        if (Helping)
        {
            Hide();
            Helping = false;
        }

    }
    RectTransform RectTransform;
    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Inside && !Helping && LastAction + 0.5 < Time.time)
        {
            Show();
        }
    }
    string KeyParser()
    {
        if (!HelpElement)
            return Key;
        switch (advance)
        {
            case HelpShowClass.BaseRegiment: return ((IHelpBaseRegiment)).BaseRegiment.ToString(); 
            case HelpShowClass.Technology:  return ((IHelpTechology)).Technology.description;
            case HelpShowClass.Person:  return ((IHelpPerson)).Person.ToString();
             case HelpShowClass.Building: return ((IHelpBuilding)).BuildingDescribe();
            default: return Key;
        }
    }
    private void Show()
    {

            Helping = true;
            HelpManager.Help(RectTransform, direction, size, KeyParser());
            LastHelper = this;
    }
    private void Hide()
    {

        HelpManager.Hide();
        Helping = false;
        LastHelper = null;
    }
}
public enum HelpPanelDirection
{
    UpRight,
    UpLeft,
    DownRight,
    DownLeft
}
public enum HelpPanelSize
{
    Little200x60,
    Medium200x200,
    Big350x300
}
public enum HelpShowClass
{
    Null,
    Person,
    Technology,
    BaseRegiment,
    Building
}
