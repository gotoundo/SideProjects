using UnityEngine;
using System.Collections.Generic;

//Do not instantiate
//but why though???
public class BasicUpgrade : MonoBehaviour {
    public enum ID { ForgeWeapons1,ForgeWeapons2,ForgeWeapons3,Base1,Base2,Base3,BazaarHealingPotion, ForgeArmor1, ForgeArmor2,ForgeArmor3,
    WeaponEnchantment1,WeaponEnchantment2,WeaponEnchantment3,ArmorEnchantment1,ArmorEnchantment2,ArmorEnchantment3, ProficiencyImplant}
    new public string name;
    public ID id;
    public int Cost;
    public float ResearchTime;
    public List<BasicItem> ItemsUnlockedForSale;
    public List<BasicAbility> AbilitiesUnlockedForSale;
    public List<ID> UpgradesRequired;
    public int RequiredBuildingLevel;
    public bool Researching = false;
    bool FinishedResearching = false;
    float remainingResearchTime;
    public bool permanentUpgrade = false;
    public List<BasicItem.Enchantment> ItemEnchantments;
    public BasicUnit researcher;
    
    public int GetCost()
    {
        return (int)(Cost * researcher.team.researchCostMultiplier);
    }

    public bool IsResearching()
    {
        return Researching;
    }

    public float ResearchPercentage()
    {
        return 1 - (remainingResearchTime / ResearchTime);
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
        return IsVisible() && researcher.team.Gold >= GetCost() && !Researching &&!FinishedResearching;
    }

    public void StartResearch()
    {
        researcher.team.Gold -= GetCost();
        remainingResearchTime = ResearchTime * researcher.team.researchTimeMultiplier;
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
        researcher.ItemEnchantmentsSold.AddRange(ItemEnchantments);

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
