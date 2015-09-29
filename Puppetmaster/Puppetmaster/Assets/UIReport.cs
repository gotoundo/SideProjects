using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIReport : MonoBehaviour {

    // Use this for initialization

    public static int Reds;
    public static int Blues;
    public Text reportText;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Reds = 0;
        Blues = 0;
        GameObject[] baseArray = GameObject.FindGameObjectsWithTag("Tile");
        foreach(GameObject o in baseArray)
        {
            TileScript tile = o.GetComponent<TileScript>();
            if (tile.Team.Equals(Color.red))
                Reds++;
            else
                Blues++;
        }

        reportText.text = "Reds: " + Reds + "   Blues: " + Blues;
    }
}
