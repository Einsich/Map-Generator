using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitMenu : MonoBehaviour
{
    public ListFiller toGarnison, toRecruit, toQueue;

    public void UpdateGarnison()
    {
        var list = Player.selected.data.garnison;
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
        var list = Player.selected.data.recruitQueue;
        toQueue.UpdateList(list.ConvertAll(x => (object)x));
    }

    public void AddToQueue(BaseRegiment regiment)
    {
        RecruitAction act = new RecruitAction(Player.selected.data, regiment, regiment.time);
        Player.selected.data.AddRecruit(act);
        UpdateQueue();
    }

    public void RemoveFromQueue(RecruitAction act)
    {
        act.actually = false;
        Player.selected.data.RemoveRecruit(act);
        UpdateQueue();
    }
}
