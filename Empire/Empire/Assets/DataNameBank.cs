using UnityEngine;
using System.Collections;

public class DataNameBank : MonoBehaviour {
    [TextArea(1, 10)]
    public string NameBank;

    char[] delimiterChars = {',' };


    public string GetRandomName()
    {
        string[] nameArray = NameBank.Split(delimiterChars);
        return nameArray[Random.Range(0, nameArray.Length)];
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
