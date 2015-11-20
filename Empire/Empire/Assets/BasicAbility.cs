using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicAbility : MonoBehaviour {

	public enum Shape {Single,Explosion}

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
    public float explosionRadius = 0;

	public List<BasicUnit> summonedUnits;
    public int maxSummonedUnits;
    public List<BasicUnit> existingSummonedUnits;

	public bool consumesTargets = false;
	public bool abilityOnlyHeals = false;
	List<BasicUnit> targets;
	public int levelRequired = 0;
	public Dictionary<BasicUnit,int> structureLevelsRequired;

    public ParticleSystem CastingVFX;
    public ParticleSystem EffectLaunchSourceVFX;
    public ParticleSystem EffectLandTargetVFX;
    public ParticleSystem EffectLandPrimaryTargetVFX;

    public AudioClip EffectLaunchSourceSFX;
    public AudioClip EffectLandPrimaryTargetSFX;

    public BasicProjectile projectile;
    public float projectileSpeed = 30;

    //learn requirements
    public bool learnedAtStructure;
    public float learnTime;
    public int minLevelToLearn;
    public BasicUnit.Stat requiredStat;
    public int requiredStatValue;


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

        CreateVFX(EffectLaunchSourceVFX, Source);
        PlaySFX(EffectLaunchSourceSFX, Source);
        foreach (BasicUnit targetUnit in targets)
        {
            if (projectile == null)
                ApplyEffectInShape(targetUnit);
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
        ApplyEffectInShape(targetUnit);
    }

    void ApplyEffectInShape(BasicUnit primaryTarget)
    {
        CreateVFX(EffectLandPrimaryTargetVFX, primaryTarget);
        PlaySFX(EffectLandPrimaryTargetSFX, primaryTarget);
        switch (shape)
        {
            case Shape.Single:
                ApplyEffect(primaryTarget);
                break;
            case Shape.Explosion:
                List<BasicUnit> validTargets = new List<BasicUnit>();
                Collider[] colliders = Physics.OverlapSphere(primaryTarget.gameObject.transform.position, explosionRadius);
                for(int i = 0; i<colliders.Length;i++)
                {
                    BasicUnit possibleUnit = colliders[i].gameObject.GetComponent<BasicUnit>();
                    if (possibleUnit != null && isValidTarget(possibleUnit))
                        validTargets.Add(possibleUnit);
                }
                foreach (BasicUnit target in validTargets)
                    ApplyEffect(target);
                break;
            default:
                break;
        }
    }

    void ApplyEffect(BasicUnit targetUnit) //for when the projectile lands
    {
        CreateVFX(EffectLandTargetVFX, targetUnit);
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
			if (potentialTarget.HasTag(tag, Source))
				acceptableTarget = true;
		
		foreach (BasicUnit.Tag tag in requiredTargetTags)
			if (!potentialTarget.HasTag(tag, Source))
				acceptableTarget = false;
		
		foreach (BasicUnit.Tag tag in excludedTargetTags)
			if (potentialTarget.HasTag(tag, Source))
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
