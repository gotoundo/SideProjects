using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	[ExecuteInEditMode]
	public class UIWindowTitle : MonoBehaviour
	{
		public string title;
		
		public void OnValidate()
		{
			foreach (Text text in this.gameObject.GetComponentsInChildren<Text>())
				text.text = this.title;
		}
	}
}