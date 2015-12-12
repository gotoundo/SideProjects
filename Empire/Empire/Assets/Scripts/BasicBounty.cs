using UnityEngine;
using System.Collections.Generic;

public class BasicBounty : MonoBehaviour {
    //all bounties are owned by the player
    
    public enum Type { Kill, Explore, Defend};

    public Type type;
    public int reward;

    public void initializeKillBounty(int reward, Transform killTarget)
    {
        initializer(Type.Kill, reward);
        transform.SetParent(killTarget);
    }

    public void initializeExploreBounty(int reward, Vector3 exploreLocation)
    {
        initializer(Type.Explore, reward);
        transform.position = exploreLocation;
    }

    public void initializeDefendBounty(int reward, Transform defendTarget)
    {
        initializer(Type.Defend, reward);
        transform.SetParent(defendTarget);
    }

    void initializer(Type type, int reward)
    {
        this.type = type;
        IncreaseBounty(reward);
        GameManager.Main.AllBounties.Add(this);
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
        GameManager.Main.AllBounties.Remove(this);
        Destroy(gameObject);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	
	}
}
