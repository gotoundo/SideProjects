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
	public enum Effect {None, Damage, Healing, Stun, Confusion}

	//Specified in the Inspector
	public Effect effect = Effect.Damage;
	public BasicUnit.Stat statUsed = BasicUnit.Stat.Strength;
	public BasicUnit.Attribute attributeUsed = BasicUnit.Attribute.None;
	public float statRatio = 1; //for example, if 1.5, use Int * 1.5 for immediate damage
	public float statPerSecRatio = 0;
	public float duration = 0; //0 for immediate effects
    public List<BasicUnit.Tag> TemporaryTags;


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
	}
	
	// Update is called once per frame
	void Update () {
		TickEffect (false);
	}


	void TickEffect(bool firstTurn)
	{
        if (firstTurn && TemporaryTags != null)
            foreach (BasicUnit.Tag tag in TemporaryTags)
                Target.AddTag(tag);


		float effectPower = totalEffectPower * (firstTurn ? statRatio : statPerSecRatio * Time.deltaTime);

		switch (effect) {
		case Effect.Damage:
			if(Source!=null)
				Source.DealDamage(effectPower,Target);
			else
				Target.TakeDamage(effectPower,null);
			break;
		case Effect.Healing:
			if(Source!=null)
				Source.DealHealing(effectPower,Target);
			else
				Target.TakeHealing(effectPower,null);
			break;
		default:
			break;
		}	


		duration = Tools.DecrementTimer (duration);
		if (duration <= 0)
			EndBuff ();
	}

	public void EndBuff()
	{
        if (Target != null && TemporaryTags != null)
            foreach (BasicUnit.Tag tag in TemporaryTags)
                Target.RemoveTag(tag);

		Destroy (gameObject);
	}

}
