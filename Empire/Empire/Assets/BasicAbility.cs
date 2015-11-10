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
	public bool finished;

	public bool Running()
	{
		return running;
	}


	public void Awake()
	{

		ResetAbility ();
	}

	public void Start()
	{
		//Debug.Log ("Ability initialized, setting running to false");

		Source = GetComponentInParent<BasicUnit> ();
		validTargetTags = validTargetTags ?? new List<BasicUnit.Tag>();
		requiredTargetTags = requiredTargetTags ?? new List<BasicUnit.Tag>();
		excludedTargetTags = excludedTargetTags ?? new List<BasicUnit.Tag>();
		summonedUnits = summonedUnits ?? new List<BasicUnit> ();
		remainingCooldown = cooldown;
	}

	public void Update()
	{
		if (!running) {
			//Debug.Log("Cooling down...");
			remainingCooldown = Tools.DecrementTimer (remainingCooldown);
		}
		else {
			if(casting)
				CastingLogic();
			else if(channeling)
				ChannelLogic();
			else
				FinishAbility();
		}
	}

	public bool CanCast()
	{

		if (remainingCooldown <= 0) {

		//	Debug.Log ("Cooldown is ready!");
			return true;
		} /*else
			Debug.Log ("Cooldown not ready :(");*/
		return false;
	}

	public void ResetAbility() //run this when selected
	{
	//	Debug.Log("Resetting Ability");
		running = false;
		casting = false;
		channeling = false;
		finished = false;
		targets = new List<BasicUnit> ();

	}

	public void StartCasting(BasicUnit target)
	{
		//Debug.Log ("StartCasting()");
		running = true;
		casting = true;
		channeling = false;
		remainingCastTime = castTime;
		targets.Add (target);
	}

	void CastingLogic()
	{
		//Debug.Log ("CastingLogic()");
		remainingCastTime = Tools.DecrementTimer (remainingCastTime);
		if(remainingCastTime <= 0)
			StartChanneling();
	}

	void StartChanneling()
	{
		//Debug.Log ("StartChanneling()");
		//Source.GetComponent<LineRenderer> ().enabled = true;
		casting = false;
		channeling = true;

		remainingChannelTime = channelTime;

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
			if(initialTarget!=null)
			{
				unit.transform.position = initialTarget.gameObject.transform.position;
			}
			if(Source!=null)
				unit.GetComponent<BasicUnit>().team = Source.team;
		}
	}

	void ChannelLogic()
	{
//		Debug.Log ("ChannelLogic()");
		LineRenderer lineRenderer = Source.gameObject.GetComponent<LineRenderer> ();
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, Source.gameObject.transform.position);
		lineRenderer.SetPosition(1, initialTarget.transform.position);

		remainingChannelTime = Tools.DecrementTimer (remainingChannelTime);
		if (remainingChannelTime <= 0)
			channeling = false;
	}

	public void FinishAbility()
	{
		//Debug.Log ("FinishAbility() - setting running to false");
		if (Source != null) {
			Source.GetComponent<LineRenderer> ().enabled = false;
		}

		remainingCooldown = cooldown;
		finished = true;
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
