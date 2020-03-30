using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradePanel : MonoBehaviour
{
    [Header("Gold,Manpower,Wood,Iron,Science")]
    [SerializeField] private Button[] SellRes;
    [SerializeField] private Button MakeDeal;
    [SerializeField] private Dropdown BuyRes;
    [SerializeField] private Slider SellCount;
    [SerializeField] private Text MaxSell, CurSell, CurBuy;
    private State CurState;
    private ResourcesType SellT, BuyT;
    private float SellR;
    private Deal CurDeal;
    void Start()
    {
        for (int i = 0; i < (int)ResourcesType.Count; i++)
        {
            ResourcesType type = (ResourcesType)i;
            SellRes[i].onClick.AddListener(() => { SellT = type; UpdateInfo(); });
        }
        BuyRes.onValueChanged.AddListener((k) => { BuyT = (ResourcesType)k; UpdateInfo(); });
        SellCount.onValueChanged.AddListener((x) => { SellR = x; UpdateInfo(); });
        MakeDeal.onClick.AddListener(AcceptDeal);
    }
    private void OnDisable()
    {
        if (CurState != null)
            GlobalTrade.BufferChanged -= UpdateInfo;
    }
    public void Show(State state)
    {
        CurState = state;
        GlobalTrade.BufferChanged += UpdateInfo;
        UpdateInfo();
    }
    private void UpdateInfo()
    {
        float maxsell = Mathf.Min(CurState.treasury[SellT] * GlobalTrade.MaxSellPercent, GlobalTrade.BufferRes(BuyT)*GlobalTrade.GetCource(SellT,BuyT));
        SellCount.maxValue = maxsell;
        if (SellR > maxsell)
            SellR = maxsell;

        MaxSell.text = maxsell.ToString();
        CurSell.text = $"продаем {SellR.ToString()} {SellT}";
        CurBuy.text = $"покупаем {SellR * GlobalTrade.GetCource(BuyT, SellT)} {BuyT}";
    }
    void AcceptDeal()
    {
        CurDeal = new Deal(CurState, SellT, SellR, BuyT, SellR * GlobalTrade.GetCource(BuyT, SellT));
        GlobalTrade.MakeDeal(CurDeal);
    }
}
