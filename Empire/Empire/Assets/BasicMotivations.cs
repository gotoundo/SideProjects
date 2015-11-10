using UnityEngine;
using System.Collections.Generic;

public class BasicMotivations : MonoBehaviour {

	[System.Serializable]
	public class Motive
	{
		public BasicUnit.State state;
		public float weight;
	}
	public List<Motive> Motives;

	void Awake()
	{
	//	if(Motives==null) Motives = new Motive[];
	}

}
