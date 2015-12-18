using UnityEngine;
using System.Collections.Generic;

public class BasicBounty : MonoBehaviour {
    //all bounties are owned by the player
    
    public enum Type { Kill, Explore, Defend};

    public Type type;
    public int reward;
    public UIBountyIcon iconTemplate;
    UIBountyIcon myIcon;
    public BasicUnit targetUnit;

    public void initializeKillBounty(int reward, BasicUnit killTarget)
    {
        initializer(Type.Kill, reward);
        transform.SetParent(killTarget.gameObject.transform);
        transform.localPosition = Vector3.zero;
        targetUnit = killTarget;
    }

    public void initializeExploreBounty(int reward, Vector3 exploreLocation)
    {
        initializer(Type.Explore, reward);
        transform.position = exploreLocation;
    }

    public void initializeDefendBounty(int reward, BasicUnit defendTarget)
    {
        initializer(Type.Defend, reward);
        transform.SetParent(defendTarget.gameObject.transform);
        transform.localPosition = Vector3.zero;
        targetUnit = defendTarget;
    }

    void initializer(Type type, int reward)
    {
        this.type = type;
        IncreaseBounty(reward);
        GameManager.Main.AllBounties.Add(this);
        myIcon = Instantiate(iconTemplate.gameObject).GetComponent<UIBountyIcon>();
        myIcon.Follow(gameObject);
    }

    public void IncreaseBounty(int goldAmount)
    {
        reward += goldAmount;
        GameManager.Main.Player.Gold -= reward;
    }

    public void ClaimExploreBounty(BasicUnit claimant)
    {
        claimant.Gold += reward;
        Remove();
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        GameManager.Main.AllBounties.Remove(this);
        if (myIcon)
            Destroy(myIcon.gameObject);
    }
}
