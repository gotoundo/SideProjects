using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicAbility : MonoBehaviour {

	public enum Shape {Single,Circle}

	//Specified in Inspector
	public List<BasicUnit.Tag> validTargetTags;
	public List<BasicUnit.Tag> requiredTargetTags;
	public List<BasicUnit.Tag> excludedTargetTags;
    public List<BasicBuff> immediateBuffsPlaced;
	public List<BasicBuff> initialBuffsPlaced;
	public Shape shape;
	public float range;
	public float castTime;
	public float channelTime;
	public float cooldown;
    public bool DrawLine = false;

	public List<BasicUnit> summonedUnits;
    public int maxSummonedUnits;
    public List<BasicUnit> existingSummonedUnits;

	public bool consumesTargets = false;
	public bool abilityOnlyHeals = false;
	List<BasicUnit> targets;
	public int levelRequired = 0;
	public Dictionary<BasicUnit,int> structureLevelsRequired;

    public ParticleSystem CastingVFX;
    public ParticleSystem EffectStartSourceVFX;
    public ParticleSystem EffectStartTargetVFX;

    public AudioClip EffectStartSourceSFX;

    public BasicProjectile projectile;
    public float projectileSpeed = 30;


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


    void CreateVFX(ParticleSystem effect, BasicUnit target)
    {
        if (effect != null)
        {
            GameObject newEffect = Instantiate(effect.gameObject);
            newEffect.transform.SetParent(target.transform);
            newEffect.transform.localPosition = new Vector3(0,1,0);

            if (effect.simulationSpace == ParticleSystemSimulationSpace.World)
                newEffect.transform.SetParent(null);

            if(Source!=null)
                newEffect.transform.rotation = Source.transform.rotation;
            Destroy(newEffect, effect.duration);
        }
    }

    void PlaySFX(AudioClip clip, BasicUnit target)
    {
        if(clip!=null)
        {
            //   AudioSource.PlayClipAtPoint(clip, target.transform.position,5f);
            AssetManager.Main.audioSource.PlayOneShot(clip);
        }
    }

    
    public void Awake()
	{

		ResetAbility ();
        existingSummonedUnits = new List<BasicUnit>();
    }

	public void Start()
	{
		//Debug.Log ("Ability initialized, setting running to false");

		Source = GetComponentInParent<BasicUnit> ();
        transform.localPosition = Vector3.zero;
		validTargetTags = validTargetTags ?? new List<BasicUnit.Tag>();
		requiredTargetTags = requiredTargetTags ?? new List<BasicUnit.Tag>();
		excludedTargetTags = excludedTargetTags ?? new List<BasicUnit.Tag>();
		summonedUnits = summonedUnits ?? new List<BasicUnit> ();
		remainingCooldown = cooldown;

	}

	public void Update()
	{
        while (existingSummonedUnits.Contains(null))
            existingSummonedUnits.Remove(null);

		if (!running) {
			remainingCooldown = Tools.DecrementTimer (remainingCooldown);
		}
		else {

            if(initialTarget == null)
            {
                FinishAbility();
                return;
            }

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
        if (summonedUnits.Count>0 && existingSummonedUnits.Count >= maxSummonedUnits)
            return false;

        return remainingCooldown <= 0;
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


    //Casting Phase

	public void StartCasting(BasicUnit target)
	{
		//Debug.Log ("StartCasting()");
		running = true;
		casting = true;
		channeling = false;
		remainingCastTime = castTime;
        targets = new List<BasicUnit>();
		targets.Add (target);

        CreateVFX(CastingVFX, Source);

        foreach (BasicUnit targetUnit in targets)
        {
            
            foreach (BasicBuff buff in immediateBuffsPlaced)
            {
                if (targetUnit != null)
                {
                    BasicBuff newBuff = Instantiate(buff).GetComponent<BasicBuff>();
                    newBuff.Setup(Source, targetUnit);
                    newBuff.gameObject.transform.SetParent(targetUnit.gameObject.transform);
                }
            }
        }
    }

	void CastingLogic()
	{
		remainingCastTime = Tools.DecrementTimer (remainingCastTime);
		if(remainingCastTime <= 0)
			StartChanneling();
	}

    //Channeling Phase

	void StartChanneling()
	{
		casting = false;
		channeling = true;
		remainingChannelTime = channelTime;

        CreateVFX(EffectStartSourceVFX, Source);
        PlaySFX(EffectStartSourceSFX, Source);
        foreach (BasicUnit targetUnit in targets)
        {
            if (projectile == null)
                ApplyEffect(targetUnit);
            else
                CreateProjectile(targetUnit);
        }
	}

    void CreateProjectile(BasicUnit targetUnit)
    {
        GameObject projectileObject = Instantiate(projectile.gameObject);
        projectileObject.transform.position = transform.position + new Vector3(0,1,0);
        BasicProjectile projectileInfo = projectileObject.GetComponent<BasicProjectile>();
        projectileInfo.Initialize(targetUnit, projectileSpeed, this);
    }

    public void ProjectileLanded(BasicUnit targetUnit)
    {
        ApplyEffect(targetUnit);
    }

    void ApplyEffect(BasicUnit targetUnit) //for when the projectile lands
    {
        CreateVFX(EffectStartTargetVFX, targetUnit);
        foreach (BasicBuff buff in initialBuffsPlaced)
        {
            if (targetUnit != null)
            {
                BasicBuff newBuff = Instantiate(buff).GetComponent<BasicBuff>();
                newBuff.Setup(Source, targetUnit);
                newBuff.gameObject.transform.SetParent(targetUnit.gameObject.transform);
            }
        }

        foreach (BasicUnit summonedUnit in summonedUnits)
        {
            if (Source != null && initialTarget != null)
            {
                BasicUnit spawnedUnit = Source.Spawn(summonedUnit.gameObject, initialTarget.gameObject.transform.position);
                existingSummonedUnits.Add(spawnedUnit);
            }
        }
    }

    void ChannelLogic()
	{
        if (DrawLine)
        {
            LineRenderer lineRenderer = Source.gameObject.GetComponent<LineRenderer>();
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, Source.gameObject.transform.position);
            lineRenderer.SetPosition(1, initialTarget.transform.position);
        }

		remainingChannelTime = Tools.DecrementTimer (remainingChannelTime);
		if (remainingChannelTime <= 0)
			channeling = false;
	}


    //Finish Phase

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


    //Helpers

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

    void OnDestroy()
    {
        foreach (BasicUnit unit in existingSummonedUnits)
            if (unit != null)
                Destroy(unit.gameObject);
    }

}
