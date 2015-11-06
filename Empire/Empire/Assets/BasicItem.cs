using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class BasicItem : MonoBehaviour
{
    public enum ItemType { Sword, Shield, Armor, Consumable }
    public ItemType Type;
    public int Level;
    public int Cost;

    [System.Serializable]
    public class AttributeEffect
    {
        public BasicUnit.Attribute attribute;
        public float value;
    }

    public AttributeEffect[] AttributeEffects;
}
