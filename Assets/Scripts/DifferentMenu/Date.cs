using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Date : MonoBehaviour {

    public Text datetext;
    public Image indicator;
    public Texture2D ind;
    public static bool cheat = false;
    public static PriorityQueue<Action> actionQueue = new PriorityQueue<Action>();
    float t;
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)&&!cheat)
            Pause();
		if(play)
        {
            fdate += speed*Time.deltaTime;
            if (fdate-curdate>=1)
            {
                t = Time.time;
                deltadate = (int)fdate - curdate;
                curdate =(int)fdate;
                UpdateDate();
            }

        }
	}
    static int[] tday = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    static int[] sday = new int[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
    static string[] nday = new string[] { "Января", "Февраля", "Марта", "Апреля", "Мая", "Июня", "Июля",
        "Августа", "Сентября", "Октября", "Ноября", "Декабря" };
    static bool play = false;
    public static int curdate, deltadate;
    static int cd,cm,cy,totalM,speed;
    public static double fdate;
    public static int dayPerSecond { get { return play ? speed : 0; } }
    public void StartTimer(int d,int m,int y)
    {
        curdate = DateToDay(d,m,y);
        fdate = curdate;
        cd = d-1;cm = m-1;cy = y;
        totalM = 0;
        speed  = 0;
        AddSpeed(12);
        datetext.text = string.Format("{0}-ого {1} {2}", cd + 1, nday[cm], cy);

        foreach (var x in Main.regions)
            x.data?.CalculateIncome();
    }
    static int DateToDay(int d,int m,int y)
    {
       return y * 365 + sday[m - 1] + d-1;
    }
    static void DayToDate(int t,out int d, out int m, out int y)
    {
        y = t / 365;
        t %= 365;
        d = m = 0;
        for(int i=0;i<12;i++)
            if(i+1==12||sday[i+1]>t)
            {
                m = i;
                d = t - sday[i];
                return;
            }
    }
    public void AddSpeed(int delta)
    {
        //day per second
        speed = Mathf.Clamp(speed + delta, 2, 10);
        UpdateIndicator();
    }
    public void Pause()
    {
        play = !play;
        UpdateIndicator();
    }
    void UpdateIndicator()
    {
        int b = (play ? 0 : 315) + (speed / 2 - 1) * 63;
        indicator.sprite = Sprite.Create(ind, new Rect(b, 0, 63, 63), new Vector2(0.5f, 0.5f));
    }
    public void UpdateDate()
    {
        DayToDate(curdate, out cd, out cm, out cy);
        datetext.text = string.Format("{0}-ого {1} {2}", cd + 1, nday[cm], cy);
        if (deltadate > 0)
            DayUpdate();
        if(cd==0)
        {
            MonthUpdate();
        }
    }
    public static void DayUpdate()
    {
        while(actionQueue.Count>0&&actionQueue.Peek().time<=curdate)
        {
            Action action = actionQueue.Dequeue();
            if (!action.actually)
                continue;
            switch (action.type)
            {
                
                case TypeAction.BuildAction:
                    BuildAction act = (BuildAction)action;
                        act.prdata.BuildComplete();
                    break;
                case TypeAction.RecruitAction:
                    RecruitAction recact = (RecruitAction)action;
                        recact.prdata.RecruitRegiment(recact);
                    break;
                case TypeAction.PersonAliveAction:
                    ((PersonAliveAction)action).person.Alive();
                    break;
            }
        }
        Battle.ProcessAllBattles();
        Army.ProcessAllArmy();
        BattleInterface.needUpdate = true;
        ProvinceMenu.needUpdate = true;

    }
    public static void MonthUpdate()
    {
        totalM++;
        for (int i = 0; i < Main.regions.Count; i++)
            Main.regions[i].MonthUpdate(totalM);
        Army.ProcessAllArmyAI();
    }
}
