using UnityEngine;
using System.Collections.Generic;

//Do not instantiate
public class BasicUpgrade : MonoBehaviour {
    public enum ID { Forge1,Forge2,Forge3}

    public ID id;
    public int Cost;
    public float ResearchTime;
    public List<BasicItem> ItemsUnlockedForSale;
    public List<ID> UpgradesRequired;
    public int RequiredBuildingLevel;
    bool Researching = false;
    float remainingResearchTime;

    public BasicUnit researcher;

    public bool IsResearching()
    {
        return Researching;
    }

    public float ResearchPercentage()
    {
        return remainingResearchTime / ResearchTime;
    }
    
    public bool IsVisible()
    {
        bool visible = RequiredBuildingLevel >= researcher.Level;
        foreach(ID requiredUpgradeID in UpgradesRequired)
        {
            if (!researcher.ResearchedUpgrades.Contains(requiredUpgradeID))
                visible = false;
        }
        return visible;
    }

    public bool CanUpgrade()
    {
        return IsVisible() && researcher.Gold >= Cost;
    }

    public void StartResearch()
    {
        researcher.Gold -= Cost;
        remainingResearchTime = ResearchTime;
        Researching = true;
    }

    void EndResearch()
    {
        Researching = false;
        researcher.AvailableUpgrades.Remove(this);
        researcher.ResearchedUpgrades.Add(id);
        researcher.ProductsSold.AddRange(ItemsUnlockedForSale);
        Destroy(gameObject);
    }

    void Update()
    {
        if(Researching)
        {
            remainingResearchTime = Time.deltaTime;
            if(remainingResearchTime <= 0)
                EndResearch();
        }
    }
}
