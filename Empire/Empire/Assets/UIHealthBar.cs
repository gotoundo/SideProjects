using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHealthBar : MonoBehaviour {

    public BasicUnit myUnit;
    Slider slider;
    public Image fill;
	// Use this for initialization
	void Start () {
        slider = gameObject.GetComponent<Slider>();
        transform.SetParent(GameManager.Main.HealthBarFolder.transform);
	}
	
	// Update is called once per frame
	void Update () {

        SetColor();

        if (myUnit == null)
            Destroy(gameObject);
        else
        {
            //fill.gameObject.SetActive(!myUnit.Tags.Contains(BasicUnit.Tag.Dead));
            FoWTileInfo tileinfo = FoWManager.FindInstance().GetTileFromWorldPosition(myUnit.transform.position);
            if (myUnit.HasTag(BasicUnit.Tag.Dead) || myUnit.HasTag(BasicUnit.Tag.Inside) || tileinfo.IsHidden)
                transform.localScale = Vector3.zero;
            else
                transform.localScale = new Vector3(1, 1, 1);

            //gameObject.SetActive(!myUnit.Tags.Contains(BasicUnit.Tag.Dead));
            transform.position = Camera.main.WorldToScreenPoint(myUnit.transform.position + new Vector3(0,0, -1));// + new Vector3(0, -myUnit.transform.localScale.y*10, 0
            slider.value = myUnit.getHealthPercentage;
        }
	}

    void SetColor()
    {
        if (myUnit.team == GameManager.Main.Player)
            fill.color = Color.green;
        else
            fill.color = Color.grey;
    }
}
