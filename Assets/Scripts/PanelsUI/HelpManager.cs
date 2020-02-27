using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpManager : MonoBehaviour
{
    public static HelpManager Instance;
    public Image panel;
    public Text information;
    public RectTransform RectTransform; 

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    const float d = 20;
    static Vector2[] pivots = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
    static Vector2[] offset = { new Vector2(d, d), new Vector2(-d, d), new Vector2(d, -d), new Vector2(-d, -d) };
    static Vector2[] size = { new Vector2(200, 60),new Vector2(200,200), new Vector2(350, 300) };
    public static void Help(RectTransform rect,HelpPanelDirection direction, HelpPanelSize panelSize, string Key)
    {
        Instance.gameObject.SetActive(true);
        var parent = Instance.transform.parent;
        Instance.transform.SetParent(rect);
        Instance.RectTransform.anchoredPosition =offset[(int)direction];
        Instance.RectTransform.pivot = pivots[(int)direction];
        Instance.RectTransform.sizeDelta = size[(int)panelSize];
        Instance.information.rectTransform.sizeDelta = size[(int)panelSize] - Vector2.one*10;
        Instance.transform.SetParent(parent);
        Instance.information.text = GetText(Key);
    }
    public static void Hide()
    {
        Instance.gameObject.SetActive(false);

    }
    static string GetText(string Key)
    {
        string[] key = Key.Split(' ');

        switch(key[0])
        {
            case "Resources":  return Player.curPlayer.ResourcesHelp(int.Parse(key[1])); break;
            case "RegimentPips":return PipsText(key[1]);
            case "Skill":return ArmyPanel.Instance.GetSkill(int.Parse(key[1]))?.ToString();
            default:return Key;
        }

    }
    static string PipsText(string key)
    {
        switch(key)
        {
            case "Melee":return "Чем выше уровень тем меньше урон от атак отрядов ближнего боя"; break;
            case "Charge": return "Чем выше уровень тем меньше урон от атак отрядов дальнего боя"; break;
            case "Range": return "Чем выше уровень тем меньше урон от наскока кавалерии"; break;
            case "Siege": return "Чем выше уровень тем меньше урон от метательных орудий"; break;
            case "Damage": return "Чем выше уровень тем больше урон"; break;
        }
        return key;
    }
}
