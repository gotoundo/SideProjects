using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIToggleDescription : MonoBehaviour {

	
    public void Click()
    {

        if (!UIInspectorPanel.Main.ToggleDescriptionMode() && GameManager.Main.PlacementMode)
            GameManager.Main.CancelPlacement();
    }
}
