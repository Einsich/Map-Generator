using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleResult : MonoBehaviour
{
    public Text[] unitsCount,unitsLoses,unitsResult,general;
    public Image[] flags;
    public Text result;
    static Stack<BattleResult> pool = new Stack<BattleResult>();
    static int open = 0;
    public static void ShowResult(Battle battle,bool win)
    {
        BattleResult result = pool.Count == 0 ? Instantiate(BattleInterface.instance.battleResultPrefab, MainMenu.that.transform) : pool.Pop();
        result.gameObject.SetActive(true);
        result.GetComponent<RectTransform>().anchoredPosition = new Vector2((open % 15) * -10f, (open % 15) * 6f);
        result.SetValue(battle, win);
        open++;
    }
    public void Hidden()
    {
        gameObject.SetActive(false);
        pool.Push(this);
        open--;
    }

    public void SetValue(Battle battle,bool win)
    {
        for (int k = 0; k < 2; k++)
        {
            Vector3Int count, loses, result;
            count = battle.StartCount[k];
            result = battle.GetCount(k);
            loses = result - count;
            Fill(unitsCount, 3 * k, count);
            Fill(unitsLoses, 3 * k, loses);
            Fill(unitsResult, 3 * k, result);
            general[k].text = battle.army[k].genegal.name;
            flags[k].sprite = battle.army[k].owner.flagSprite;
        }
        result.text = win ? "Мы победили." : "Мы проиграли";
        result.color = win ? Color.green : Color.red;
    }
    void Fill(Text[] text, int i, Vector3Int v)
    {
        text[i + 0].text = v.x.ToString();
        text[i + 1].text = v.y.ToString();
        text[i + 2].text = v.z.ToString();
    }
    void Update()
    {
        
    }
}
