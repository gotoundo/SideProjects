using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIResearchButton : MonoBehaviour
{
    public Text buttonText;
    public BasicUpgrade upgrade;


    void Update()
    {
        GetComponent<Button>().interactable = upgrade.CanUpgrade();
    }

    public void Click()
    {
        if (upgrade.CanUpgrade())
        {
            upgrade.StartResearch();
            GameManager.Main.MenuAction();
        }
    }
}
