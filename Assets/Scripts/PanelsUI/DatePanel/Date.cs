using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Date : MonoBehaviour {

    public Text datetext;
    public Image indicator;
    public Texture2D ind;
    public static bool cheat = false;
    public static bool play = true;
    int speed;
    static float LastDeciUpdate,LastSecondUpdate, LastDecaUpdate;
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && !cheat)
            Pause();
    }
    private void FixedUpdate()
    {
		if(play)
        {
            GameTimer.time += speed * 0.2f * Time.fixedDeltaTime;
            GameTimer.RealTimeUpdate();
            if (LastDeciUpdate + 0.1f <= GameTimer.time)
            {
                GameTimer.DeciSecondUpdate();
                UpdateDate();
                LastDeciUpdate = GameTimer.time;
            }
            if (LastSecondUpdate + 1f <= GameTimer.time)
            {
                GameTimer.EverySecondUpdate();
                LastSecondUpdate = GameTimer.time;
            }
            if (LastDecaUpdate + 10f <= GameTimer.time)
            {
                GameTimer.DecaSecondUpdate();
                LastDecaUpdate = GameTimer.time;

            }

        }
	}
    public void StartTimer()
    {
        AddSpeed(5);
        UpdateDate();
        GameTimer.Start();

    }
    public void AddSpeed(int delta)
    {
        speed = Mathf.Clamp(speed + delta, 1, 5);
        GameTimer.timeScale = speed * 0.2f;
        UpdateIndicator();
    }
    public void Pause()
    {
        play = !play;
        UpdateIndicator();
        foreach (var x in Army.AllArmy)
            x.enabled = play;
    }
    void UpdateIndicator()
    {
        int b = (play ? 0 : 315) + (speed-1) * 63;
        indicator.sprite = Sprite.Create(ind, new Rect(b, 0, 63, 63), new Vector2(0.5f, 0.5f));
    }
    public void UpdateDate()
    {
        int s = (int)GameTimer.time;
        int m = s / 60;
        s %= 60;
        int h = m / 60;
        m %= 60;
        if (h != 0)
            datetext.text = string.Format("{0}:{1:D2}:{2:D2}", h, m, s);
        else
            datetext.text = string.Format("{0}:{1:D2}", m, s);

    }
}
