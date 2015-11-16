using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBuyBounty : MonoBehaviour {

    public BasicBounty BountyTemplate;
    Button UIButton;

    // Use this for initialization
    void Start()
    {
        UIButton = GetComponent<Button>();
        UIButton.GetComponentInChildren<Text>().text = BountyTemplate.type.ToString() + " (" + GameManager.defaultBountyIncrement + " gold)";
    }

    // Update is called once per frame
    void Update()
    {
        UIButton.interactable = GameManager.Main.Player.CanAffordBounty();
        
    }
    public void BuyBounty()
    {
        GameManager.Main.Player.EnterPlaceExploreBountyMode(BountyTemplate);
    }
}
