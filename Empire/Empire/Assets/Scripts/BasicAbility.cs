using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DigitalRuby.ThunderAndLightning;

public class BasicAbility : MonoBehaviour {
    [System.Serializable]
    public class LaunchProfile
    {
        public string attachmentPoint = "LaunchPoint";
        public float delay = 0f;
        public bool fired = false;
        public void Reset()
        {
            fired = false;
        }
    }

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
    public List<LaunchProfile> Launches;
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

    public GameObject LightningTemplate;
    //public string launchAttachmentPoint = "LaunchPoint";

    //Learning Requirements
    public int levelRequired = 0;
    //public bool learnedAtStructure;
    //public Dictionary<BasicUnit,int> structureLevelsRequired;
    public float learnTime;
    public int learnCost;
    public BasicUnit.Stat requiredStat;
    public int requiredStatValue;
    

    //Specified by Logic, they should all be private but are public for inspector debugging
    float remainingCooldown;
	public float remainingChannelTime;
	float remainingCastTime;
	bool running;
	bool casting;
	bool channeling;
	bool finished;
    bool nullTarget;

    public bool Finished { get { return finished; } }
    public bool Running { get { return running; } }

    //Locals
    const string defaultLaunchPoint = "LaunchPoint";
    const string defaultCastPoint = "LaunchPoint";
    const string defaultImpactPoint = "ImpactPoint";

    BasicUnit Source;
    List<BasicUnit> targets;
    LightningBoltPrefabScript lightningScript;


   // GameObject LaunchPoint;


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


    void CreateVFX(ParticleSystem effect, BasicUnit target, string attachmentName)
    {
        if (effect != null)
        {
            Vector3 location = target.transform.position + new Vector3(0, target.GetHeight() / 2, 0); //default location is in middle of object

            GameObject attachmentPoint = GetAttachmentPoint(target.gameObject, attachmentName);
            if (attachmentPoint)
                location = attachmentPoint.transform.position;
            else
                attachmentPoint = target.gameObject;

            GameObject newEffect = (GameObject)Instantiate(effect.gameObject, location, target.transform.rotation);

            if (effect.simulationSpace == ParticleSystemSimulationSpace.Local)
                newEffect.transform.SetParent(attachmentPoint.transform);

            if (Source != null)
                newEffect.transform.rotation = Source.transform.rotation;

            Destroy(newEffect, effect.duration);
        }
    }

    void CreateLightning(GameObject effect, BasicUnit target, string launchPoint)
    {
        if (effect != null)
        {
            GameObject newEffect = (GameObject)Instantiate(effect, target.transform.position, target.transform.rotation);
            lightningScript = newEffect.GetComponent<LightningBoltPrefabScript>();
            lightningScript.Source = GetAttachmentPoint(target.gameObject,launchPoint);
            lightningScript.Destination = target.gameObject;
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

    GameObject GetAttachmentPoint(GameObject objectBody, string attachmentName)
    {
        if (objectBody == null)
            return gameObject;

        GameObject attachmentObject = AdamTools.FindInChildren(objectBody, attachmentName);

        if (attachmentObject)
            return attachmentObject;

        return objectBody;
    }

	public void Start()
	{
        //Debug.Log ("Ability initialized, setting running to false");
        //LaunchPoint = GetAttachmentPoint(launchAttachmentPoint);


        Source = GetComponentInParent<BasicUnit> ();
        transform.localPosition = Vector3.zero;
		validTargetTags = validTargetTags ?? new List<BasicUnit.Tag>();
		requiredTargetTags = requiredTargetTags ?? new List<BasicUnit.Tag>();
		excludedTargetTags = excludedTargetTags ?? new List<BasicUnit.Tag>();
		summonedUnits = summonedUnits ?? new List<BasicUnit> ();
		remainingCooldown = cooldown;

        if (Launches == null || Launches.Count == 0)
        {
            Launches = new List<LaunchProfile>();
            Launches.Add(new LaunchProfile());
        }

	}

	public void Update()
	{
        if (GameManager.Running)
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
        if (!Unlocked())
            return false;

        return remainingCooldown <= 0;
	}

    public bool Unlocked()
    {
        if (summonedUnits.Count > 0 && existingSummonedUnits.Count >= maxSummonedUnits)
            return false;

        if (Source.Level < levelRequired)
            return false;

        return true;
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
        /*Transform launchTransform = Source.transform.Find(launchAttachmentPoint);
        if (launchTransform)
            Debug.Log(Source.name + " found its " + launchAttachmentPoint);
        else
            Debug.Log(Source.name + " failed to find its " + launchAttachmentPoint);*/

        //Debug.Log ("Casting "+name);
        running = true;
        casting = true;
        channeling = false;
        remainingCastTime = castTime;
        remainingChannelTime = channelTime;
        targets = new List<BasicUnit>();
        targets.Add(target);

        CreateVFX(CastingVFX, Source,defaultCastPoint);
        
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

        foreach (LaunchProfile profile in Launches)
            profile.Reset();

        CheckLaunches();
    }

    void CheckLaunches()
    {
        foreach(LaunchProfile launchProfile in Launches)
        {
            if (!launchProfile.fired && channelTime - remainingChannelTime >= launchProfile.delay)
                LaunchEffect(launchProfile);
        }
    }

    void LaunchEffect(LaunchProfile launchProfile)
    {
        launchProfile.fired = true;

        CreateVFX(EffectLaunchSourceVFX, Source,launchProfile.attachmentPoint);

        PlaySFX(EffectLaunchSourceSFX, Source);
        foreach (BasicUnit targetUnit in targets)
        {
            if (projectile == null)
                ApplyEffectInShape(targetUnit);
            else
                CreateProjectile(targetUnit,launchProfile.attachmentPoint);
            
            if (LightningTemplate)
                CreateLightning(LightningTemplate, targetUnit,launchProfile.attachmentPoint);
        }
    }

    void CreateProjectile(BasicUnit targetUnit, string launchPoint)
    {
        Vector3 startPoint = transform.position + new Vector3(0, Source.GetHeight() / 2, 0); //default starting location is at center of source
        GameObject attachPoint = GetAttachmentPoint(Source.gameObject,launchPoint); //what happens if Source != null   :-0
        if (attachPoint != Source.gameObject)
            startPoint = attachPoint.transform.position;


        GameObject projectileObject = (GameObject)Instantiate(projectile.gameObject, startPoint, Source.transform.rotation);
        BasicProjectile projectileInfo = projectileObject.GetComponent<BasicProjectile>();
        projectileInfo.Initialize(targetUnit, projectileSpeed, this);
    }

    public void ProjectileLanded(BasicUnit targetUnit)
    {
        ApplyEffectInShape(targetUnit);
    }

    void ApplyEffectInShape(BasicUnit primaryTarget)
    {
        CreateVFX(EffectLandPrimaryTargetVFX, primaryTarget,defaultImpactPoint);
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
        CreateVFX(EffectLandTargetVFX, targetUnit, defaultImpactPoint);
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

        CheckLaunches();

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

        if (lightningScript)
            Destroy(lightningScript.gameObject);

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
