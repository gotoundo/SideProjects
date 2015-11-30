using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class BasicItem : MonoBehaviour
{
    new public string name;
    public Image icon;
    public enum ItemType { Claws, Shield, LightArmor,HeavyArmor, Potion, Rifle, RPG, Pistol, Flamethrower, HealingDevice,Shotgun,
    ArchitectTool,AvatarWeapon,Fists, NaturalWeapon,StasisWeapon}
    public ItemType Type;
    public int Level;
    public int Cost;
	public bool Consumable = false;
    public int EnchantmentLevel;
    //public string EnchantmentString;

    public static List<ItemType> EnchantableWeaponTypes = new List<ItemType>(new ItemType[] { ItemType.Claws,ItemType.Rifle,ItemType.RPG,ItemType.Pistol,ItemType.Flamethrower,ItemType.HealingDevice,ItemType.Shotgun,ItemType.Fists,ItemType.NaturalWeapon,ItemType.StasisWeapon });
    public static List<ItemType> EnchantableArmorTypes = new List<ItemType>(new ItemType[] {  ItemType.LightArmor,ItemType.HeavyArmor});

    public string GetName()
    {
        string output = name;
        if (EnchantmentLevel > 0)
            output = "+" + EnchantmentLevel + " " + output;
        return output;
    }

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

    [System.Serializable]
    public class Enchantment
    {
        public int EnchantCost;
        public bool EnchantAllWeapons;
        public bool EnchantAllArmors;
        public List<BasicItem.ItemType> OtherEnchantTypes;
        public List<BasicItem.ItemType> GetEnchantTypes()
        {
            List<BasicItem.ItemType> types = new List<ItemType>();
            if (OtherEnchantTypes != null)
                types.AddRange(OtherEnchantTypes);
            if (EnchantAllArmors)
                types.AddRange(EnchantableArmorTypes);
            if (EnchantAllWeapons)
                types.AddRange(EnchantableWeaponTypes);

            return types;
            
        }
        public int EnchantLevel = 0;
    }
}
