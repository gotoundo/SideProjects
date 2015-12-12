using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBuildUnit : MonoBehaviour
{

    public Text buttonText;
    public BasicUnit.UnitSpawner spawner;


    void Update()
    {
        GetComponent<Button>().interactable = spawner.CanSpawn();
        if(spawner.hiring)
            buttonText.text = spawner.SpawnType.templateID + " (" + (int)(100 * spawner.HiringPercentage()) + "%)";
        else
            buttonText.text = spawner.SpawnType.templateID + " (" + spawner.SpawnType.ScaledGoldCost() + ")";
    }

    public void Click()
    {
        if (spawner.CanSpawn())
        {
            spawner.StartHiring();
            GameManager.Main.MenuAction();
        }
    }
}
