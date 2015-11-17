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
        if (myUnit.Tags.Contains(BasicUnit.Tag.Monster))
            fill.color = Color.grey;
	}
	
	// Update is called once per frame
	void Update () {



        if (myUnit == null)
            Destroy(gameObject);
        else
        {
            slider.enabled = !myUnit.Tags.Contains(BasicUnit.Tag.Dead);
            transform.position = Camera.main.WorldToScreenPoint(myUnit.transform.position) + new Vector3(0, -myUnit.transform.localScale.y*10, 0);
            slider.value = myUnit.currentHealth / myUnit.getMaxHP;
        }
	}
}
