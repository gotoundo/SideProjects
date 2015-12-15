using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingDemonstration : MonoBehaviour {
	
	public Text label;
	public float duration = 5f;
	public float wait = 1f;
	
	protected void Start()
	{
		if (this.label != null)
			this.StartCoroutine("StartDemo");
	}
	
	IEnumerator StartDemo()
	{
		if (this.label == null)
			yield break;
			
		float startTime = Time.time;
		
		while (Time.time <= (startTime + this.duration))
		{
			this.label.text = Mathf.RoundToInt(((Time.time - startTime) / this.duration) * 100f).ToString() + "%";
			yield return null;
		}
		
		yield return new WaitForSeconds(this.wait);
		this.StartCoroutine("StartDemo");
	}
}
