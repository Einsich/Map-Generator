using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitMenu : MonoBehaviour
{
    public ListFiller toGarnison, toRecruit, toQueue;
    static RecruitMenu instance;
    private void Awake()
    {
        instance = this;
    }
    public void UpdateGarnison()
    {
        var list = Player.curRegion.data.garnison;
        toGarnison.UpdateList(list.ConvertAll(x=>(object)x));
    }

    List<RecruitListElement> recList = new List<RecruitListElement>();
    List<RecruitQueueElement> recQueue = new List<RecruitQueueElement>();
    List<GarnisonIcon> garList = new List<GarnisonIcon>();
    public  void ShowRecruitMenu(Region region)
    {
        var list = region.owner.regiments;
        toRecruit.UpdateList(list.ConvertAll(x => (object)x));
        UpdateQueue();
    }
    void UpdateQueue()
    {
        var list = Player.curRegion.data.recruitQueue;
        toQueue.UpdateList(list.ConvertAll(x => (object)x));
    }
    public static void AddToQueue(BaseRegiment regiment)
    {
        RecruitAction act = new RecruitAction(Player.curRegion.data, regiment, regiment.time);
        Player.curRegion.data.AddRecruit(act);
        instance.UpdateQueue();
    }

    public static void RemoveFromQueue(RecruitAction act)
    {
        act.actually = false;
        Player.curRegion.data.RemoveRecruit(act);
        instance.UpdateQueue();
    }
}
