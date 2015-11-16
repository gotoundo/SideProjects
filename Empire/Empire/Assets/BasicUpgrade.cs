using UnityEngine;
using System.Collections.Generic;

//Do not instantiate
public class BasicUpgrade : MonoBehaviour {
    public enum ID { ForgeWeapons1,ForgeWeapons2,ForgeWeapons3,Base1,Base2,Base3,BazaarHealingPotion}

    public ID id;
    public int Cost;
    public float ResearchTime;
    public List<BasicItem> ItemsUnlockedForSale;
    public List<ID> UpgradesRequired;
    public int RequiredBuildingLevel;
    bool Researching = false;
    bool FinishedResearching = false;
    float remainingResearchTime;
    public bool permanentUpgrade = false;

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
        bool visible = RequiredBuildingLevel <= researcher.Level;
        foreach(ID requiredUpgradeID in UpgradesRequired)
        {
            if (!researcher.ResearchedUpgrades.Contains(requiredUpgradeID))
                visible = false;
        }
        return visible;
    }

    public bool CanUpgrade()
    {
        return IsVisible() && researcher.team.Gold >= Cost && !Researching &&!FinishedResearching;
    }

    public void StartResearch()
    {
        researcher.team.Gold -= Cost;
        remainingResearchTime = ResearchTime;
        Researching = true;
        GameManager.Main.PossibleOptionsChange(researcher);
    }

    void EndResearch()
    {
        Researching = false;
        FinishedResearching = true;
        researcher.AvailableUpgrades.Remove(this);
        researcher.ResearchedUpgrades.Add(id);
        researcher.ProductsSold.AddRange(ItemsUnlockedForSale);

        researcher.team.TeamUpgrades.Add(id);

        GameManager.Main.PossibleOptionsChange(researcher);
        Destroy(gameObject);
    }

    void Update()
    {
        if(Researching)
        {
            remainingResearchTime -= Time.deltaTime;
            if(remainingResearchTime <= 0)
                EndResearch();
        }
    }

    void OnDestroy()
    {
        if (!permanentUpgrade)
            GameManager.Main.Player.TeamUpgrades.Remove(id);
    }

    
}
