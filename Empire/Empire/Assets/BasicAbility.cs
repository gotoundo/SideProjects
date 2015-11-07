using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicAbility : MonoBehaviour {

	public enum Shape {Single,Circle}

	//Specified in Inspector
	public List<BasicUnit.Tag> validTargetTags;
	public List<BasicUnit.Tag> requiredTargetTags;
	public List<BasicUnit.Tag> excludedTargetTags;
	public List<BasicBuff> initialBuffsPlaced;
	public Shape shape;
	public float range;
	public float castTime;
	public float channelTime;
	public float cooldown;
	public List<BasicUnit> summonedUnits;
	public bool consumesTargets = false;
	public bool abilityOnlyHeals = false;
	List<BasicUnit> targets;
	public int levelRequired = 0;
	public Dictionary<BasicUnit,int> structureLevelsRequired;

	BasicUnit Source;

	BasicUnit initialTarget 
	{ 
		get
		{
			return targets[0]; 
		}
	}

	 //summonedUnits

	//Specified by Logic, they should all be private
	public float remainingCooldown;
	public float remainingChannelTime;
	public float remainingCastTime;
	public bool running;
	public bool casting;
	public bool channeling;

	public bool Running()
	{
		return running;
	}



	public void Start()
	{
		running = false;
		casting = false;
		channeling = false;
		Source = GetComponentInParent<BasicUnit> ();
		validTargetTags = validTargetTags ?? new List<BasicUnit.Tag>();
		requiredTargetTags = requiredTargetTags ?? new List<BasicUnit.Tag>();
		excludedTargetTags = excludedTargetTags ?? new List<BasicUnit.Tag>();
	}

	public void Update()
	{
		if (!casting)
			remainingCooldown = Mathf.Max (0, remainingCooldown - Time.deltaTime);
		else {
			if(casting)
				CastingLogic();
			if(channeling)
				ChannelLogic();
			if(channelTime <= 0)
				FinishAbility();
		}
	}

	public bool CanCast()
	{
		return remainingCooldown > 0;
	}

	public void StartCasting(BasicUnit target)
	{
		running = true;
		casting = true;
		channeling = false;

		remainingCastTime = castTime;
		targets = new List<BasicUnit> ();
		targets.Add (target);
	}

	void CastingLogic()
	{
		remainingCastTime -= Time.deltaTime;
		if(remainingCastTime <= 0)
			StartChanneling();
	}

	void StartChanneling()
	{
		casting = false;
		channeling = true;

		foreach (BasicUnit targetUnit in targets) {
			foreach(BasicBuff buff in initialBuffsPlaced)
			{
				if(targetUnit != null)
				{
					BasicBuff newBuff = Instantiate(buff).GetComponent<BasicBuff>();
					newBuff.Setup(Source,targetUnit);
					newBuff.gameObject.transform.SetParent(targetUnit.gameObject.transform);
				}
			}
		}

		foreach(BasicUnit summonedUnit in summonedUnits)
		{
			GameObject unit = Instantiate(summonedUnit.gameObject);
			if(Source!=null)
				unit.GetComponent<BasicUnit>().team = Source.team;
		}
	}

	void ChannelLogic()
	{
		Debug.Log ("Channeling!!");
		LineRenderer lineRenderer = Source.GetComponent<LineRenderer> ();
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, transform.position);
		lineRenderer.SetPosition(1, initialTarget.transform.position);

		remainingChannelTime -= Time.deltaTime;
		if(remainingChannelTime <= 0)
			FinishAbility();
	}

	public void FinishAbility()
	{
		if (Source != null) {
			Source.GetComponent<LineRenderer> ().enabled = false;
		}

		remainingCooldown = cooldown;
		casting = false;
		channeling = false;
		running = false;
		if(consumesTargets)
			foreach(BasicUnit unit in targets)
				Destroy(unit.gameObject);
	}


	public bool isValidTarget(BasicUnit potentialTarget)
	{
		bool acceptableTarget = false;
		
		foreach (BasicUnit.Tag tag in validTargetTags)
			if (potentialTarget.Tags.Contains(tag))
				acceptableTarget = true;
		
		foreach (BasicUnit.Tag tag in requiredTargetTags)
			if (!potentialTarget.Tags.Contains(tag))
				acceptableTarget = false;
		
		foreach (BasicUnit.Tag tag in excludedTargetTags)
			if (potentialTarget.Tags.Contains(tag))
				acceptableTarget = false;
		
		if (abilityOnlyHeals && potentialTarget.AtMaxHealth())
			acceptableTarget = false;

		return acceptableTarget;
	}

	public bool isWithinRange(BasicUnit potentialTarget)
	{
		return Vector3.Distance(Source.gameObject.transform.position, potentialTarget.gameObject.transform.position) <= range;
	}

}
