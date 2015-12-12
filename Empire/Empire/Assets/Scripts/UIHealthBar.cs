using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHealthBar : MonoBehaviour {

    BasicUnit myUnit;
    Slider slider;
    public Image fill;
    public Text nameText;
    
    public void Initialize(BasicUnit unit)
    {
        myUnit = unit;
    }

	// Use this for initialization
	void Start () {
        slider = gameObject.GetComponent<Slider>();
        transform.SetParent(GameManager.Main.HealthBarFolder.transform);
	}
	
	// Update is called once per frame
	void Update () {

        

        if (myUnit == null)
            Destroy(gameObject);
        else
        {
            SetColor();

            if(myUnit.HasTag(BasicUnit.Tag.Hero))
                nameText.text = myUnit.name + " ("+myUnit.Level+")";

            //Decide to show or hide
            FoWTileInfo tileinfo = FoWManager.FindInstance().GetTileFromWorldPosition(myUnit.transform.position);
            if (myUnit.HasTag(BasicUnit.Tag.Dead) || myUnit.HasTag(BasicUnit.Tag.Inside) || tileinfo.IsHidden)
                transform.localScale = Vector3.zero;
            else //show
            {
                float scale = GameManager.Main.CameraHeightScale();
                transform.localScale = new Vector3(scale, scale, 1);
            }

            //Set position
            transform.position = Camera.main.WorldToScreenPoint(myUnit.transform.position + new Vector3(0,0, -myUnit.GetHeight()));
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
