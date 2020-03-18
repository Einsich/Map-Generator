using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiplomacyPanel : MonoBehaviour
{

    public ListFiller DiplomacyFiller;
    List<DiplomacyProxi> list = new List<DiplomacyProxi>();
    Diplomacy diplomacy;
    public void UpdateInfo()
    {
        list.Clear();
        foreach (var d in Diplomacy.diplomacies)
            if (d != diplomacy)
            {
                list.Add(new DiplomacyProxi(diplomacy, d));
            }

        list.Sort((x, y) => x.relation > y.relation ? -1 : x.relation < y.relation ? 1 : 0);
        DiplomacyFiller.UpdateList(list.ConvertAll(x => (object)x));
    }
    public void Show(Diplomacy diplomacy)
    {
        this.diplomacy = diplomacy;
        UpdateInfo();
    }
}
