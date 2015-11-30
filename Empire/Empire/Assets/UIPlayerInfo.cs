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
        if(GameManager.Playing)
        {
            PlayerGoldAmount.text = "Credits: "+GameManager.Main.Player.Gold;
        }
	}
}
