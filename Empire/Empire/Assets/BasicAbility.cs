using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicAbility : MonoBehaviour {

	public enum Shape {Single,Explosion}
    public new string name;

	//Targeting
	public List<BasicUnit.Tag> validTargetTags;
	public List<BasicUnit.Tag> requiredTargetTags;
	public List<BasicUnit.Tag> excludedTargetTags;
    public List<BasicUnit.Tag> initialRequiredTargetTags;
    public List<BasicUnit.Tag> initialExcludedTargetTags;
    public int maxLevelTargeted = -1;

    //Base Mechanics
    public List<BasicBuff> immediateBuffsPlaced;
	public List<BasicBuff> initialBuffsPlaced;
	public Shape shape;
	public float range;
	public float castTime;
	public float channelTime;
	public float cooldown;
    public bool DrawLine = false;
    public float explosionRadius = 0;

    //Summoning
	public List<BasicUnit> summonedUnits;
    public int maxSummonedUnits;
    public List<BasicUnit> existingSummonedUnits;

    //Options
	public bool consumesTargets = false;
	public bool abilityOnlyHeals = false;
	
    //VFX & SFX
    public ParticleSystem CastingVFX;
    public ParticleSystem EffectLaunchSourceVFX;
    public ParticleSystem EffectLandTargetVFX;
    public ParticleSystem EffectLandPrimaryTargetVFX;

    public AudioClip EffectLaunchSourceSFX;
    public AudioClip EffectLandPrimaryTargetSFX;

    public BasicProjectile projectile;
    public float projectileSpeed = 30;

    //Learning Requirements
    public int levelRequired = 0;
    //public bool learnedAtStructure;
    //public Dictionary<BasicUnit,int> structureLevelsRequired;
    public float learnTime;
    public int learnCost;
    public BasicUnit.Stat requiredStat;
    public int requiredStatValue;
    

    //Specified by Logic, they should all be private but are public for inspector debugging
    public float remainingCooldown;
	public float remainingChannelTime;
	public float remainingCastTime;
	public bool running;
	public bool casting;
	public bool channeling;
	public bool finished;
    public bool nullTarget;

    //Locals
    BasicUnit Source;
    List<BasicUnit> targets;

    public bool CanBeLearnedBy(BasicUnit student)
    {
        return student.GetStat(requiredStat) >= requiredStatValue && student.Level >= levelRequired;
    }

    BasicUnit initialTarget
    {
        get
        {
            return targets[0];
        }
    }


    public bool Running()
	{
		return running;
	}


    void CreateVFX(ParticleSystem effect, BasicUnit target)
    {
        if (effect != null)
        {
            GameObject newEffect = (GameObject)Instantiate(effect.gameObject,target.transform.position,target.transform.rotation);
            newEffect.transform.SetParent(target.transform);

            if (effect.simulationSpace == ParticleSystemSimulationSpace.World)
                newEffect.transform.SetParent(null);

            if(Source!=null)
                newEffect.transform.rotation = Source.transform.rotation;
            Destroy(newEffect, effect.duration);
        }
    }

    void PlaySFX(AudioClip clip, BasicUnit target)
    {
        if (clip != null)
        {
            if(AssetManager.Main.audioSource != null)
                AssetManager.Main.audioSource.PlayOneShot(clip);
            else
            {
                Debug.Log("Why the fuck is AssetManager's audiosource null??");
                AudioSource.PlayClipAtPoint(clip, target.transform.position, 5f);
            }
            

            
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
        if (GameManager.Playing)
        {
            while (existingSummonedUnits.Contains(null))
                existingSummonedUnits.Remove(null);

            if (!running)
            {
                remainingCooldown = Tools.DecrementTimer(remainingCooldown);
            }
            else
            {
                nullTarget = initialTarget == null;
                if (!nullTarget)
                {
                    if (casting)
                        CastingLogic();
                    else if (channeling)
                        ChannelLogic();
                    else
                        FinishAbility();
                }
            }
        }
	}

	public bool CanCast()
	{
        if (summonedUnits.Count>0 && existingSummonedUnits.Count >= maxSummonedUnits)
            return false;

        if(Source.Level < levelRequired)
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
		//Debug.Log ("Casting "+name);
		running = true;
		casting = true;
		channeling = false;
		remainingCastTime = castTime;
        remainingChannelTime = channelTime;
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
        Source.SetAnimationState("Firing", true);
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
        GameObject projectileObject = (GameObject)Instantiate(projectile.gameObject, transform.position + new Vector3(0,Source.GetHeight()/2,0),Source.transform.rotation);
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
                    if (possibleUnit != null && IsValidTarget(possibleUnit, true))
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
		if (Source != null) {
            if(Source.debugMode)
                Debug.Log("FinishAbility() - setting running to false");
            Source.GetComponent<LineRenderer> ().enabled = false;
            Source.SetAnimationState("Firing", false);
        }

		remainingCooldown = cooldown;
        remainingCastTime = 0;
        remainingChannelTime = 0;
        finished = true;
		casting = false;
		channeling = false;
		running = false;
        
		if(consumesTargets)
			foreach(BasicUnit unit in targets)
				Destroy(unit.gameObject);
	}


    //Helpers

	public bool IsValidTarget(BasicUnit potentialTarget, bool initialCasting)
	{
        if (potentialTarget == null)
            return false;

        if (potentialTarget.HasTag(BasicUnit.Tag.Inside) && !validTargetTags.Contains(BasicUnit.Tag.Inside))
            return false;

		bool acceptableTarget = false;
		
        //Initial Matching
		foreach (BasicUnit.Tag tag in validTargetTags)
			if (potentialTarget.HasTag(tag, Source))
				acceptableTarget = true;
		
        //Required Tags
		foreach (BasicUnit.Tag tag in requiredTargetTags)
			if (!potentialTarget.HasTag(tag, Source))
				acceptableTarget = false;

        if(initialCasting)
            foreach (BasicUnit.Tag tag in initialRequiredTargetTags)
                if (!potentialTarget.HasTag(tag, Source))
                    acceptableTarget = false;

        //Excluded Tags
        foreach (BasicUnit.Tag tag in excludedTargetTags)
			if (potentialTarget.HasTag(tag, Source))
				acceptableTarget = false;

        if(initialCasting)
            foreach (BasicUnit.Tag tag in initialExcludedTargetTags)
                if (potentialTarget.HasTag(tag, Source))
                    acceptableTarget = false;

        if (abilityOnlyHeals && potentialTarget.AtMaxHealth())
			acceptableTarget = false;

        if (maxLevelTargeted != -1 && potentialTarget.Level > maxLevelTargeted)
            acceptableTarget = false;

		return acceptableTarget;
	}



    void OnDestroy()
    {
        foreach (BasicUnit unit in existingSummonedUnits)
            if (unit != null)
                Destroy(unit.gameObject);
    }

}
