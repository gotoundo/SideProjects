using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPlayerInfo : MonoBehaviour {
    public Text PlayerGoldAmount;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(GameManager.Main.Running)
        {
            PlayerGoldAmount.text = "Gold: "+GameManager.Main.Player.Gold;
        }
	}
}
