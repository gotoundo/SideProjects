using UnityEngine;
using System.Collections.Generic;


public abstract class TraitModifier {
	public Dictionary<BasicUnit.Attribute,float> AttributeModifiers; //derived from stats
	public Dictionary<BasicUnit.Stat,float> StatModifiers;

	public TraitModifier()
	{
		AttributeModifiers = new Dictionary<BasicUnit.Attribute, float> ();
		StatModifiers = new Dictionary<BasicUnit.Stat, float> ();
	}
}
