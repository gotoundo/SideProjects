using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;


public class BasicUnit : MonoBehaviour,  IPointerClickHandler{
    public bool debugMode = false;
    
    //Enumerations
    public enum Tag { Structure, Organic, Imperial, Monster, Mechanical, Dead, Store, Self, Hero, Consumed}
    public enum State { None, Deciding, Exploring, Hunting, InCombat, Following, Shopping, GoingHome, Fleeing, Relaxing, Sleeping, Dead, Structure, Stunned } //the probabilities of which state results after "Deciding" is determined per class
	public enum Stat{ Strength, Dexterity, Intelligence, Special}
	public enum Attribute { None, MaxHealth, PhysicalDamage, MagicDamage, MoveSpeed, AttackSpeed, HealthRegen, MaxMana, ManaRegen }


    //Attributes and Stats
	Dictionary<Attribute, float> baseAttributes;
	Dictionary<Stat, float> baseStats;
	public float initialStrength;
	public float initialDexterity;
	public float initialIntelligence;
	public float initialSpecial; //used for structure abilities
	public float levelUpStrength;
	public float levelUpDexterity;
	public float levelUpIntelligence;
	public float levelUpSpecial;

	void initializeStatsAndAttributes()
	{
		baseStats = new Dictionary<Stat, float> ();
		baseStats.Add (Stat.Strength, initialStrength);
		baseStats.Add (Stat.Intelligence, initialIntelligence);
		baseStats.Add (Stat.Dexterity, initialDexterity);
		baseStats.Add (Stat.Special, initialSpecial);

		baseAttributes = new Dictionary<Attribute, float> ();
		baseAttributes.Add (Attribute.AttackSpeed, 0);
		baseAttributes.Add (Attribute.HealthRegen, 0);
		baseAttributes.Add (Attribute.MagicDamage, 0);
		baseAttributes.Add (Attribute.ManaRegen, 0);
		baseAttributes.Add (Attribute.MaxHealth, 0);
		baseAttributes.Add (Attribute.MaxMana, 0);
		baseAttributes.Add (Attribute.MoveSpeed, agent?agent.speed:0);
		baseAttributes.Add (Attribute.None, 0);
		baseAttributes.Add (Attribute.PhysicalDamage, 0);
	}
	public float GetStat(Stat stat) //stat mods are added or subtracted
	{
		float baseStat = baseStats [stat];

		for (int i = 0; i< EquipmentSlots.Count(); i++) {
			BasicItem item = EquipmentSlots[i].Instance;
			if(item!=null)
			{
				for(int j =0; j <item.StatEffects.Count(); j++)
					if(item.StatEffects[j].stat == stat)
						baseStat += item.StatEffects[j].value;
			}
		}
		return baseStat;
	}
	public float GetAttribute(Attribute attribute) //attribute modifiers are added or subtracted
    {
		float baseAttribute = baseAttributes [attribute];
		
		for (int i = 0; i< EquipmentSlots.Count(); i++) {
			BasicItem item = EquipmentSlots[i].Instance;
			if(item!=null)
			{
				for(int j =0; j <item.AttributeEffects.Count(); j++)
					if(item.AttributeEffects[j].attribute == attribute)
						baseAttribute += item.AttributeEffects[j].value;
			}
		}
		return baseAttribute;
	}

	public float getMaxHP { get { return (GetStat(Stat.Strength) * 10) + GetAttribute(Attribute.MaxHealth); } }
    
    //RPG Progression
    public int Gold;
    public float XP; //make private
    public int Level;
    public int GoldCost;

    //RPG Combat
    public float currentHealth;
    public List<BasicAbility> Abilities;
	BasicAbility currentAbility;

    [System.Serializable]
    public class EquipmentSlot
    {
        public BasicItem.ItemType Type;
        public BasicItem Instance;
    }
    public EquipmentSlot[] EquipmentSlots;
    public BasicItem[] ProductsSold; 
    
    //Core Logic
    public List<Tag> Tags;
    public State currentState;
    
    //Components
    Animator animator;
    NavMeshAgent agent;
    LineRenderer lineRenderer;
    new Renderer renderer;
    Rigidbody rigidBody;

    //Relationships
    public GameObject myTemplate;
    public string parentObjectName;
    public Team team;
    GameObject Home;
    public GameObject MoveTarget;
    BasicUnit MoveTargetUnit { get { return MoveTarget ? MoveTarget.GetComponent<BasicUnit>() : null; } }
    public Vector3 ExploreTarget;
    bool spawnedByStructure = false;

    //Search Radii
    float huntSearchRadius = 20f;
    float storeSearchRadius = 1000f;
    float deathGiftRadius = 20f;
    float cryForHelpRadius = 10f;
	public float exploreRadius = 50f;
	public bool tetheredToHome = false;
        
    //Spawning
    public bool autoSpawningEnabled = false;
    public int spawnGoldCost;
    public GameObject SpawnType;
    public int MaxSpawns = 0;
    public float SpawnCooldown = 3;
    private float RemainingSpawnCooldown;
    public List<GameObject> Spawns;

    //Timers - make private
    public float corpseDuration = 0;
    public float remainingCorpseDuration;
    public float remainingDecideTime; //not implemented yet

    //Other constants
    const float stoppingDistanceMargin = 2;
    
    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        renderer = GetComponent<Renderer>();
        Spawns = new List<GameObject>();
        Tags = Tags ?? new List<Tag>();
		Abilities = Abilities ?? new List<BasicAbility> ();

		initializeStatsAndAttributes ();
		currentHealth = getMaxHP;

		for (int i = 0; i<Abilities.Count; i++) {
			{
				Abilities[i] = Instantiate(Abilities[i]).GetComponent<BasicAbility>();
				Abilities[i].gameObject.transform.SetParent(transform);
			}
		}

        XP = Mathf.Pow(Level,2);

        if (team == null && Tags.Contains(Tag.Imperial))
            team = GameManager.Main.Player;

        if (Tags.Contains(Tag.Structure)) //Structure Setup
        {
            corpseDuration = 0;
            setNewState(State.Structure);
        }
        else //Normal Unit Setup
        {
            setNewState(State.Deciding);
            corpseDuration = 15;
            agent.stoppingDistance = 2;
        }

        lineRenderer.material.color = renderer.material.color;
    }
	
	// Update is called once per frame
	void Update () {

        switch (currentState)
        {
            case State.Deciding: //add a random duration - for structures this will be 0
                DecideLogic();
                
                break;
            case State.Exploring:
                ExploreLogic();
                break;
            case State.Hunting:
                HuntingLogic();
                break;
            case State.InCombat:
                break;
            case State.Following:
                break;
            case State.Shopping:
                ShoppingLogic();
                break;
            case State.GoingHome:
                break;
            case State.Sleeping:
                break;
            case State.Dead:
                Die();
                break;
            default:
                break;
        }

        if(!Tags.Contains(Tag.Structure))
        animator.SetBool("Walking", agent.velocity.magnitude > 0.05f);

        UpdateSpawning();
        CleanUp();
	}

    void setNewState(State newState, string reason = "")
    {
        if (debugMode)
            Debug.Log(((int)GameManager.Main.playTime) + " | " + gameObject.name + " started " + newState.ToString() +": "+reason);

        if (currentAbility != null)
            InterruptAbility();
        MoveTarget = null;
        currentAbility = null;
        ExploreTarget = Vector3.zero;
        currentState = newState;
    }

    void ExploreLogic()
    {
        if (ExploreTarget == Vector3.zero) {
			ExploreTarget = tetheredToHome && Home!=null? Home.transform.position: transform.position;
			ExploreTarget += new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f)) * exploreRadius;
			ExploreTarget = new Vector3(Mathf.Max(0,Mathf.Min(ExploreTarget.x,GameManager.Main.MapBounds.x)), transform.position.y,Mathf.Max(0,Mathf.Min(ExploreTarget.z,GameManager.Main.MapBounds.z)));
		}
        agent.SetDestination(ExploreTarget);
        if (Vector3.Distance(ExploreTarget, transform.position) < agent.stoppingDistance + stoppingDistanceMargin)
            StopExploring();
    }

    void StopExploring()
    {
        setNewState(State.Deciding);
    }

	void StartDeciding()
	{
		if(Tags.Contains(Tag.Structure))
		{
            setNewState(State.Structure);
			return;
		}
        setNewState(State.Deciding);
		remainingDecideTime = 1;
	}

    void DecideLogic()
    {
        if(Tags.Contains(Tag.Dead))
        {
            Die();
            return;
        }

        if(Tags.Contains(Tag.Structure))
        {
            setNewState(State.Structure);
            return;
        }

        float RandomSelection = Random.Range(0, 100);
        if (RandomSelection < 70)
			StartHunting ();
		else if (RandomSelection < 80)
			StartShopping ();
		else
			StartExploring ();
            
    }

	void StartShopping()
	{
        setNewState(State.Shopping);
	}

	void StartExploring()
	{
        setNewState(State.Exploring);
	}

    void ShoppingLogic()
    {

        if (MoveTargetUnit == null || !MoveTargetUnit.Tags.Contains(Tag.Store))
        {
            if (!FindStore())
                DoneShopping();
        }
        else
        {
            agent.stoppingDistance = 3;
            if (Vector3.Distance(MoveTarget.transform.position, transform.position) < agent.stoppingDistance + stoppingDistanceMargin)
            {
                agent.stoppingDistance = 100;
                BrowseWares();
            }
        }
    }

    void BrowseWares()
    {
		int amount = new List<BasicItem> (MoveTargetUnit.ProductsSold).Count; //if you put this in the for loop it fucking explodes
		for (int product = 0; product < amount; product++) {
			for (int slot = 0; slot < EquipmentSlots.Count(); slot++) {

				if (MoveTargetUnit.ProductsSold == null)
					Debug.Log (MoveTarget.name + "sells no products");

				BasicItem soldItem = MoveTargetUnit.ProductsSold [product];
				if (EquipmentSlots [slot].Type == soldItem.Type) {
					if (EquipmentSlots [slot].Instance == null || soldItem.Level > EquipmentSlots [slot].Instance.Level) {
						if (soldItem.Cost <= Gold) {
							BuyItem (soldItem, slot);
							DoneShopping ();
							return;
						}
					}
				}
			}
		}
		DoneShopping ();
	}

	void BuyItem(BasicItem soldItem, int slot)
	{
		Gold -= soldItem.Cost;
		MoveTargetUnit.GainGold(soldItem.Cost);
		EquipmentSlots[slot].Instance = Instantiate(soldItem.gameObject).GetComponent<BasicItem>();
		if(debugMode)
            Debug.Log(gameObject.name + " bought " + EquipmentSlots[slot].Instance.name + " from " + MoveTarget + ".");
	}

    void DoneShopping()
    {
        MoveTarget = null;
        currentState = State.Deciding;
    }

    bool FindStore()
    {
        List<BasicUnit> initialTargets = GetUnitsWithinRange(storeSearchRadius);
        List<BasicUnit> acceptableTargets = new List<BasicUnit>();
        foreach (BasicUnit encounteredUnit in initialTargets)
        {
            if (encounteredUnit.Tags.Contains(Tag.Store) && encounteredUnit.team == team && CheckStoreForDesireableItems(encounteredUnit).Count>0)
                acceptableTargets.Add(encounteredUnit);
        }

        if (acceptableTargets.Count > 0)
        {
            MoveTarget = acceptableTargets[Random.Range(0, acceptableTargets.Count - 1)].gameObject;
            if(debugMode)
                Debug.Log(gameObject.name + " chooses to shop at " + MoveTarget.name);
            return true;
        }

        return false;
    }
    
    void CleanUp()
    {
        if (currentHealth <= 0 && currentState != State.Dead)
            Die();

        while (Spawns.Contains(null))
            Spawns.Remove(null);
        
        //Move Towards Target's new position
        if (agent != null && agent.isActiveAndEnabled)
        {
            if (MoveTarget != null)
                agent.SetDestination(MoveTarget.transform.position);
            else if (ExploreTarget == Vector3.zero)
                agent.SetDestination(transform.position);
        }
    }

    void UpdateSpawning()
    {
        if (!autoSpawningEnabled || Spawns.Count >= MaxSpawns)
            return;

        RemainingSpawnCooldown -= Time.deltaTime;
        if(RemainingSpawnCooldown <= 0)
        {
            Spawn(SpawnType,transform.position);
        }
    }

    public bool CanSpawn()
    {
        return (spawnGoldCost <= Gold);
    }

    public BasicUnit Spawn(GameObject template, Vector3 position)
    {
        RemainingSpawnCooldown = SpawnCooldown;

        GameObject spawnedObject = (GameObject)Instantiate(template, position, transform.rotation);
        Spawns.Add(spawnedObject);
        BasicUnit spawnedUnit = spawnedObject.GetComponent<BasicUnit>();
        spawnedUnit.myTemplate = template;
        spawnedUnit.Home = gameObject;
        spawnedUnit.spawnedByStructure = true;
        spawnedUnit.parentObjectName = template.gameObject.name;
        spawnedUnit.team = team;
        spawnedObject.name = template.name;

        return spawnedUnit;
    }


    void StartHunting()
    {
        setNewState(State.Hunting);
    }

    void HuntingLogic()
    {
		if (Abilities.Count == 0) {
            if (debugMode)
                Debug.Log("Stopping Hunting because I have no abilities");
            StopHunting();
			return;
		}
        
        if(MoveTarget == null) //find unit to focus on
        {
            List<BasicUnit> acceptableTargets = chooseAbilityAndFindPossibleTargets();

            acceptableTargets = SortByDistance(acceptableTargets);

            if (acceptableTargets.Count != 0)
                MoveTarget = acceptableTargets[0].gameObject; //Random.Range(0, acceptableTargets.Count) //pick closest target
        }

        if (MoveTarget != null)
            ChannelAbility();
        else
        {
            if (debugMode)
                Debug.Log("Stopping Hunting because Move Target is null");
            StopHunting();
        }
    }

    void StopHunting()
    {
        InterruptAbility ();
        setNewState(State.Deciding);
	}

    List<BasicUnit> SortByDistance(List<BasicUnit> PotentialTargets)
    {
        return PotentialTargets.OrderBy(o => Vector3.Distance(o.transform.position,transform.position)).ToList();
    }


	public void DealDamage(float damage, BasicUnit Target)
	{
		Target.TakeDamage (damage, this);
	}

	public void DealHealing(float healing, BasicUnit Target)
	{
		Target.TakeHealing (healing, this);
	}

	void PickAbility(BasicAbility pickedAbility)
	{
		currentAbility = pickedAbility;
		currentAbility.ResetAbility();
		agent.stoppingDistance = currentAbility.range - stoppingDistanceMargin;
	}

	void PickAbility()
	{
		PickAbility (Abilities [0]);
	}

    void ChannelAbility() //only activated if cooldown is 0
    {
        agent.SetDestination(MoveTarget.transform.position);

        if (Abilities.Count == 0) {
            if (debugMode)
                Debug.Log("Stopping Hunting because I have no abilities");
            StopHunting ();
			return;
		}

        BasicUnit initialAbilityTarget = MoveTargetUnit;
		if (currentAbility == null)
			PickAbility();

        if (!currentAbility.isValidTarget(initialAbilityTarget)) {
			//Debug.Log("Initial target no longer valid! Cancelling ability use.");
			InterruptAbility ();
			return;
		}

		if (currentAbility.isWithinRange (initialAbilityTarget)) {
			//Debug.Log ("In range of ability!");
			if (currentAbility.CanCast() && !currentAbility.running) {
                
				currentAbility.StartCasting (initialAbilityTarget);
                //agent.stop
                
                //agent.SetDestination(transform.position);
                
            }
		}

        animator.SetBool("Firing",  true);//!currentAbility.finished

        if (currentAbility.finished) {

			return; //end of ability use decision logic - might not actually need anything here
		}
    }



    void InterruptAbility()
    {
        animator.SetBool("Firing", false);
        if (currentAbility != null && !currentAbility.finished) {
			Debug.Log ("Ability Interrupted");
			currentAbility.FinishAbility();
		}
		currentAbility = null;
        MoveTarget = null;
    }

    List<BasicUnit> chooseAbilityAndFindPossibleTargets()
    {
		foreach (BasicAbility potentialAbility in Abilities) {
			Collider[] hitColliders = Physics.OverlapSphere (transform.position, huntSearchRadius);
			List<BasicUnit> acceptableTargets = new List<BasicUnit> ();
			foreach (Collider collider in hitColliders) {
				BasicUnit PotentialTarget = collider.gameObject.GetComponent<BasicUnit> ();
				if (PotentialTarget != null && PotentialTarget != this) {
					bool acceptableTarget = potentialAbility.isValidTarget (PotentialTarget);

					if (acceptableTarget)
						acceptableTargets.Add (PotentialTarget);
				}
			}
			if(acceptableTargets.Count>0)
			{
				PickAbility(potentialAbility);
				//Debug.Log("Chose ability from options");
				return acceptableTargets;
			}
		}
		return new List<BasicUnit> ();
    }

    public void TakeDamage(float damage, BasicUnit source)
    {
        if(debugMode)
		Debug.Log (gameObject.name + " is taking " + damage + " damage from " + source.gameObject.name);
        currentHealth -= Mathf.Max(0,damage);
        if (currentState != State.Hunting)
            DecideLogic();
        else if (currentHealth / getMaxHP < .15f)
            DecideLogic();
    }

    public void TakeHealing(float damage, BasicUnit source)
    {
        currentHealth += Mathf.Max(0,damage);
        currentHealth = Mathf.Min(currentHealth, getMaxHP);
    }

    void Die()
    {
        setNewState(State.Dead);
        if (agent != null)
        {
            agent.speed = 0;
            agent.Stop();
        }

        if (!Tags.Contains(Tag.Dead))
        { //then initialize the death state
            
            Tags.Add(Tag.Dead);
            remainingCorpseDuration = corpseDuration;

            if (Tags.Contains(Tag.Monster))
                OnDeathDistributeGoldAndXP();

            
            if (animator != null)
                animator.SetBool("Dead", true);
            
            renderer.material.color = Color.grey;
        }

        remainingCorpseDuration -= Time.deltaTime;
        if (remainingCorpseDuration <= 0 && !Tags.Contains(Tag.Consumed))
        {
            if (Tags.Contains(Tag.Imperial))
                OnDeathDistributeGoldAndXP();
            Destroy(gameObject);
        }
    }


    void OnDeathDistributeGoldAndXP()
    {
        //Grant Gold
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, deathGiftRadius);
        List<BasicUnit> goldRecipients = new List<BasicUnit>();
        foreach (Collider col in hitColliders)
        {
            BasicUnit NearbyUnit = col.gameObject.GetComponent<BasicUnit>();
            if (NearbyUnit != null && !NearbyUnit.Tags.Contains(Tag.Structure) && !NearbyUnit.Tags.Contains(Tag.Dead) && NearbyUnit.team != team )
                goldRecipients.Add(col.gameObject.GetComponent<BasicUnit>());
        }
        int initialGold = Gold;
        for (int i = 0; i < goldRecipients.Count; i++) //distribute held gold
        {
            if (i == goldRecipients.Count - 1)
            {
                goldRecipients[i].GainGold(Gold);
                Gold = 0;
            }
            else
            {
                int goldGranted = initialGold / goldRecipients.Count;
                goldRecipients[i].GainGold(goldGranted);
                Gold -= goldGranted;
            }
        }

        //Grant XP
        List<BasicUnit> xpRecipients = new List<BasicUnit>();
        foreach (Collider col in hitColliders)
        {
            BasicUnit NearbyUnit = col.gameObject.GetComponent<BasicUnit>();
            if (NearbyUnit != null && !NearbyUnit.Tags.Contains(Tag.Structure) && !NearbyUnit.Tags.Contains(Tag.Dead) && NearbyUnit.team != team && NearbyUnit.Tags.Contains(Tag.Hero))
                xpRecipients.Add(col.gameObject.GetComponent<BasicUnit>());
        }
        foreach (BasicUnit xpRecipient in xpRecipients)
            xpRecipient.GainXP(Level / (float)goldRecipients.Count);
    }

    public bool AtMaxHealth()
    {
        return currentHealth >= getMaxHP;
    }

    void GainGold(int goldAmount)
    {
        Gold += goldAmount; //add visual and sound effects
        if(Tags.Contains(Tag.Store) && team != null)
        {
            team.Gold += Gold;
            Gold = 0;
        }
    }

    void GainXP(float xpAmount)
    {
       // Debug.Log(gameObject.name + " earned " + xpAmount + " XP");

        XP += xpAmount; 
        int proposedNewLevel = (int)Mathf.Sqrt(XP);
        while (Level < proposedNewLevel)
            LevelUp();
    }

	void LevelUp() //add visual and sound effects
    {
        Level++; 
        Debug.Log(gameObject.name + " leveled up to " + Level + "!");
        gameObject.name = (parentObjectName.Length ==0 ? gameObject.name : parentObjectName) + " " + Level;

		baseStats[Stat.Strength] += levelUpStrength;
		baseStats[Stat.Dexterity] += levelUpDexterity;
		baseStats[Stat.Intelligence] += levelUpIntelligence;
		baseStats [Stat.Special] += levelUpSpecial;

        currentHealth = getMaxHP;
    }

    void BroadcastCryForHelp(BasicUnit attacker)
    {
        List<BasicUnit> nearbyUnits = GetUnitsWithinRange(cryForHelpRadius);
        foreach(BasicUnit unit in nearbyUnits)
        {
            if (unit.team == team)
                unit.HearCryForHelp(this, attacker);
        }
    }

    public void HearCryForHelp(BasicUnit unitInDistress, BasicUnit attacker)
    {
        if(!Tags.Contains(Tag.Structure))
        {
            if(currentState == State.Exploring)
            {
                StopExploring();
                StartHunting();
            }
        }
    }

    List<BasicUnit> GetUnitsWithinRange(float range)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        List<BasicUnit> unitList = new List<BasicUnit>();
        foreach (Collider col in hitColliders)
        {
            BasicUnit NearbyUnit = col.gameObject.GetComponent<BasicUnit>();
            if (NearbyUnit != null)
                unitList.Add(NearbyUnit);
        }
        return unitList;
    }

    List<BasicItem> CheckStoreForDesireableItems(BasicUnit store)
    {
        List<BasicItem> DesireableItems = new List<BasicItem>();
        int productCount = new List<BasicItem>(store.ProductsSold).Count; //if you put this in the for loop it fucking explodes
        for (int product = 0; product < productCount; product++)
        {
            for (int slot = 0; slot < EquipmentSlots.Count(); slot++)
            {
                BasicItem soldItem = store.ProductsSold[product];
                if (EquipmentSlots[slot].Type == soldItem.Type)
                {
                    if (EquipmentSlots[slot].Instance == null || soldItem.Level > EquipmentSlots[slot].Instance.Level)
                    {
                        DesireableItems.Add(EquipmentSlots[slot].Instance);
                    }
                }
            }
        }
        return DesireableItems;
    }
    


    //UI Considerations
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Main.StartInspection(this);
    }
}
