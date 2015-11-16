using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILevelUpStructure : MonoBehaviour {

    public Text buttonText;

    void Update()
    {
        if (GameManager.Main.InspectedUnit.AnotherStructureLevelExists())
            GetComponent<Button>().interactable = GameManager.Main.InspectedUnit.CanAffordToLevelUpStructure();
        else
            Destroy(gameObject);
    }

    public void Click()
    {
        if (GameManager.Main.InspectedUnit.CanAffordToLevelUpStructure())
        {
            GameManager.Main.InspectedUnit.LevelUpStucture();
        }
    }
}
