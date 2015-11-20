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
    }

    public void Click()
    {
        if (spawner.CanSpawn())
        {
            spawner.Spawn();
            GameManager.Main.MenuAction();
        }
    }
}
