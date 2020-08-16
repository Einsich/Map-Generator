using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateStatistic : MonoBehaviour
{
    public RectTransform canvas;
    public Dropdown statisticTypeSelector;
    public GameObject ResourcesPanel;
    public Button[] ResourcesButtons;
    public ShieldUI shieldPrefab;
    void Start()
    {
        Line.SetCanvas(canvas);
        statisticTypeSelector.onValueChanged.AddListener(ChangeStatisticType);
        statisticTypeSelector.value = (int)statisticType;
        for (int i = 0; i < ResourcesButtons.Length; i++)
        {
            int type = i;
            ResourcesButtons[i].onClick.AddListener(() => ChangeResourcesType((ResourcesType)type));
        }
        (widht, height) = (canvas.sizeDelta.x, canvas.sizeDelta.y);
        InputManager.Instance[KeyCode.I] += ShowStatistic;
        gameObject.SetActive(false);
        for (int i = 0; i < shields.Length; i++)
        {
            shields[i] = Instantiate(shieldPrefab, canvas);
            shields[i].transform.localScale = Vector3.one;
            shields[i].gameObject.SetActive(false);
        }
    }
    private void ChangeStatisticType(int type)
    {
        statisticType = (StatisticType)type;
        if(statisticType != StatisticType.Income && statisticType != StatisticType.TotalTreasure)
        {
            ResourcesPanel.SetActive(false);
        } else
        {
            ResourcesPanel.SetActive(true);
        }
        ShowStates();

    }
    public void ChangeResourcesType(ResourcesType type)
    {
        showResource = type;
        ShowStates();
    }
    public void ShowStatistic()
    {
        active = !gameObject.activeSelf;
        gameObject.SetActive(active);
        if(active)
        {
            ShowStates();
        }
    }
    public static bool active;
    public static float widht, height;
    public static ShieldUI[] shields = new ShieldUI[20];
    public static StatisticType statisticType = StatisticType.Income;
    public static ResourcesType showResource = ResourcesType.Count;
    static List<(float, State)> leaders = new List<(float, State)>();
    static float GetValue(State state)
    {
        float value = 0;
        var res = state.statistica[state.statistica.Count - 1];
        switch (statisticType)
        {
            case StatisticType.Income: value = (showResource == ResourcesType.Count) ? res.normalizeIncome : res.income[showResource]; break;
            case StatisticType.TotalTreasure: value = (showResource == ResourcesType.Count) ? res.normalizeTotal : res.total[showResource]; break;
        }
        return value;
    }
    static List<State> FindLeader(int maxCount)
    {
        leaders.Clear();
        foreach(var state in Main.states)
            if(!state.destroyed)
            {
                leaders.Add((-GetValue(state), state));
            }
        leaders.Sort((a, b) => (int)Mathf.Sign(a.Item1 - b.Item1));
        List<State> ans = new List<State>(maxCount + 1);
        for (int i = 0; i < maxCount && i < leaders.Count; i++)
                ans.Add(leaders[i].Item2);
        return ans;
    }
    static void SetShieldButton(ShieldUI shield, State state)
    {

        shield.flag.sprite = state.flagSprite;
        shield.shild.onClick.RemoveAllListeners();
        shield.shild.onClick.AddListener(() => CameraController.SetTarget(state.Capital.pos));
    }
    public static void ShowStates()
    {
        if (!active)
            return;
        List<State> leaders = FindLeader(5);
        if (Player.curPlayer != null && !leaders.Contains(Player.curPlayer))
            leaders.Add(Player.curPlayer);
        LineData[] lines = new LineData[leaders.Count];
        float maxTime = 0;
        float maxValue = 0;
        for (int i = 0; i < leaders.Count; i++)
        {
            List<ResourceData> resources = leaders[i].statistica;
            int n = resources.Count;
            lines[i] = new LineData();
            List<Vector2> point = new List<Vector2>(n);
            lines[i].data = point;
            lines[i].color = leaders[i].mainColor;
            if (leaders[i] == Player.curPlayer)
                lines[i].selected = true;
            lines[i].value = GetValue(leaders[i]);
            SetShieldButton(shields[i], leaders[i]);
            switch (statisticType)
            {
                case StatisticType.Income:
                    foreach (var res in resources)
                    {
                        float value = (showResource == ResourcesType.Count) ? res.normalizeIncome : res.income[showResource];
                        float time = res.time;
                        maxTime = Mathf.Max(maxTime, time);
                        maxValue = Mathf.Max(maxValue, value);
                        point.Add(new Vector2(time, value));
                    } break;
                case StatisticType.TotalTreasure:
                    foreach (var res in resources)
                    {
                        float value = (showResource == ResourcesType.Count) ? res.normalizeTotal : res.total[showResource];
                        float time = res.time;
                        maxTime = Mathf.Max(maxTime, time);
                        maxValue = Mathf.Max(maxValue, value);
                        point.Add(new Vector2(time, value));
                    }
                    break;
            }
        }
        ShowLines(lines, new Vector2(maxTime, maxValue));
    }
    static void FindOffset(LineData[] lines, int k, float sizeDivScale)
    {
        int d = 0;
        for (int J = 0; J < 20; J++)
            for (int i = 0; i < k; i++)
                if (lines[i].deltaX == d && Mathf.Abs(lines[i].value - lines[k].value) < sizeDivScale)
                {
                    d++;
                    break;
                }
        lines[k].deltaX = d;
    }
    static void ShowLines(LineData[] lines, Vector2 size)
    {
        Line.Clear();
        Vector2 scale = new Vector2(widht / size.x, height / size.y);
        for (int j = 0; j < lines.Length; j++)
        {
            LineData line = lines[j];
            ShieldUI shield = shields[j];
            Vector2 shieldSize = shield.rectTransform.sizeDelta;
            FindOffset(lines, j, shieldSize.y / scale.y);

            shield.gameObject.SetActive(true);
            shield.rectTransform.anchoredPosition = new Vector2(widht + shield.rectTransform.sizeDelta.x * (0.5f + line.deltaX), line.value * scale.y);
            for (int i = 0; i < line.data.Count - 1; i++)
            {
                Line.GetLine().SetPoints(line.data[i] * scale, line.data[i + 1] * scale, line.color, line.selected);
            }
        }
    }
}
class Line
{
    private Image image;
    private RectTransform rect;
    private Line()
    {
        GameObject line = new GameObject("lineConnection", typeof(Image));
        line.transform.SetParent(canvas);
        rect = line.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        image = line.GetComponent<Image>();
    }
    static Stack<Line> pull = new Stack<Line>();
    static Stack<Line> active = new Stack<Line>();
    static Transform canvas;
    public static void SetCanvas(Transform canvas) => Line.canvas = canvas;

    public static Line GetLine()
    {
        if (canvas == null)
            return null;

        Line line = pull.Count == 0 ? new Line() : pull.Pop();
        active.Push(line);
        line.rect.gameObject.SetActive(true);
        line.rect.transform.SetAsLastSibling();
        return line;
    }
    public static void Clear()
    {
        while(active.Count != 0)
        {
            Line line = active.Pop();
            line.rect.gameObject.SetActive(false);
            pull.Push(line);
        }
        foreach (var shield in StateStatistic.shields)
            shield.gameObject.SetActive(false);
    }
    public void SetPoints(Vector2 a, Vector2 b, Color color, bool selected)
    {
        Vector2 d = b - a;
        rect.anchoredPosition = (a + b) * 0.5f;
        rect.sizeDelta = new Vector2(d.magnitude, selected ? 8 : 4);
        rect.localRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(d.y, d.x), Vector3.forward);
        image.color = color;
    }
}
public class LineData
{
    public bool selected = false;
    public List<Vector2> data;
    public Color color;
    public float value;
    public int deltaX;
}
public struct ResourceData
{
    public Treasury income, total;
    public float time, normalizeIncome, normalizeTotal;
    public ResourceData(Treasury Income, Treasury Total, float Time) =>
        (income, total, time, normalizeIncome, normalizeTotal) = (Income, Total, Time, Income.NormalizedToGold, Total.NormalizedToGold);

}
public enum StatisticType
{
    Income, TotalTreasure
}