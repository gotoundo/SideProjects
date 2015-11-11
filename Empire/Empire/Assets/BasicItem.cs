using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class BasicItem : MonoBehaviour
{
    public enum ItemType { Claws, Shield, Armor, Potion, Rifle}
    public ItemType Type;
    public int Level;
    public int Cost;
	public bool Consumable = false;

    [System.Serializable]
    public class AttributeEffect
    {
        public BasicUnit.Attribute attribute;
        public float value;
    }

	[System.Serializable]
	public class StatEffect
	{
		public BasicUnit.Stat stat;
		public float value;
	}

    public AttributeEffect[] AttributeEffects;
	public StatEffect[] StatEffects;
}
