using UnityEngine;
using System.Collections.Generic;

public static class Tools
{
	public static float DecrementTimer(float timer)
	{
		return Mathf.Max (0, timer - Time.deltaTime);
	}
}

public class BasicBuff : MonoBehaviour { //attached to 
	public enum Effect {None, Damage, Healing, Stun, Convert, KineticDamage,EnergyDamage,PsychicDamage,Slow}

	//Specified in the Inspector
	public Effect effect = Effect.Damage;
	public BasicUnit.Stat statUsed = BasicUnit.Stat.Strength;
	public BasicUnit.Attribute attributeUsed = BasicUnit.Attribute.None;
	public float statRatio = 1; //for example, if 1.5, use Int * 1.5 for immediate damage
	public float statPerSecRatio = 0;
	public float duration = 0; //0 for immediate effects
    
 

    public BasicItem.AttributeEffect[] AttributeEffects;
    public BasicItem.StatEffect[] StatEffects;

    public bool Stackable = true;

    public List<BasicUnit.Tag> TemporaryTags;
    public List<BasicUnit.Tag> TemporaryTagsRemoved;
    //public bool TeamChange;

    Team origionalTeam;


	//Specified when Instantiated by an Ability
	BasicUnit Source;
	BasicUnit Target;
	float totalEffectPower;
	
	// Use this for initialization
	void Start () {
		TickEffect (true);
	}

	public void Setup(BasicUnit Source, BasicUnit Target)
	{
		this.Source = Source;
		this.Target = Target;
		totalEffectPower = Source.GetStat (statUsed)+Source.GetAttribute(attributeUsed);

        if(!Stackable)
        {
            List<BasicBuff> conflictingBuffs = new List<BasicBuff>();
            foreach (BasicBuff otherBuff in Target.attachedBuffs)
                if (otherBuff.effect == effect && !otherBuff.Stackable)
                    conflictingBuffs.Add(otherBuff);

            foreach (BasicBuff conflict in conflictingBuffs)
                conflict.EndBuff();
        }
        Target.attachedBuffs.Add(this);
	}
	
	// Update is called once per frame
	void Update () {
        if (GameManager.Running)
        {
            TickEffect(false);
        }
	}


	void TickEffect(bool firstTurn)
	{
        if (firstTurn)
        {
            if(TemporaryTags != null)
                foreach (BasicUnit.Tag tag in TemporaryTags)
                    Target.AddTag(tag);
            if (TemporaryTagsRemoved != null)
                foreach (BasicUnit.Tag tag in TemporaryTagsRemoved)
                    Target.RemoveTag(tag);
            if (effect == Effect.Convert)
            {
                Target.SwitchTeams(Source.team);
            }
        }

		float effectPower = totalEffectPower * (firstTurn ? statRatio : statPerSecRatio * Time.deltaTime);

        switch (effect)
        {
            case Effect.Damage:
                DealTypedDamage(effectPower, effect);
                break;
            case Effect.EnergyDamage:
                DealTypedDamage(effectPower, effect);
                break;
            case Effect.KineticDamage:
                DealTypedDamage(effectPower, effect);
                break;
            case Effect.PsychicDamage:
                DealTypedDamage(effectPower, effect);
                break;
            case Effect.Healing:
                if (Source != null)
                    Source.DealHealing(effectPower, Target);
                else
                    Target.TakeHealing(effectPower, null);
                break;
            case Effect.Stun:
                Target.StartStun();
                break;
            default:
                break;
        }
        
		duration = Tools.DecrementTimer (duration);
		if (duration <= 0)
			EndBuff ();
	}

    void DealTypedDamage(float damage, Effect damageType)
    {
        if (Source != null)
            Source.DealDamage(damage, Target,damageType);
        else
            Target.TakeDamage(damage, null, damageType);
    }

	public void EndBuff()
	{
        if (Target != null)
        {
            if (TemporaryTags != null)
                foreach (BasicUnit.Tag tag in TemporaryTags)
                    Target.RemoveTag(tag);
            if (TemporaryTagsRemoved != null)
                foreach (BasicUnit.Tag tag in TemporaryTagsRemoved)
                    Target.AddTag(tag);

            Target.attachedBuffs.Remove(this);
          
            switch (effect)
            {
                case Effect.Stun:
                    Target.EndStun();
                    break;
                case Effect.Convert:
                    Target.SwitchTeams(origionalTeam);
                    break;
                default:
                    break;
            }
        }
		Destroy (gameObject);
	}

}
