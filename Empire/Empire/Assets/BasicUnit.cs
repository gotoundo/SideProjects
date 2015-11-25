using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;


public class BasicUnit : MonoBehaviour,  IPointerClickHandler{
    public bool debugMode = false;

    public string templateID;
    public string templateDescription;
    new public string name;
   
    //Enumerations
    public enum Tag { Structure, Organic, Imperial, Monster, Mechanical, Dead, Store, Self, Hero, Consumed, Enemy, Ally, Neutral}
    public enum State { None, Deciding, Exploring, Hunting, InCombat, Following, Shopping, GoingHome, Fleeing, Relaxing, Sleeping, Dead, Structure, Stunned, ExploreBounty,KillBounty,DefendBounty } //the probabilities of which state results after "Deciding" is determined per class
	public enum Stat{ Strength, Dexterity, Intelligence, Special}
	public enum Attribute { None, MaxHealth, PhysicalDamage, MagicDamage, MoveSpeed, AttackSpeed, HealthRegen, MaxMana, ManaRegen, Resistance}

    List<State> passiveStates = new List<State>( new State[] { State.Exploring, State.ExploreBounty, State.Shopping, State.Sleeping, State.GoingHome }); // these can be interrupted
    List<State> disabledStates = new List<State>(new State[] { State.Stunned}); // these can be interrupted

    //Attributes and Stats
    Dictionary<Attribute, float> baseAttributes;
	Dictionary<Stat, float> baseStats;

    [System.Serializable]
    public class StatBlock
    {
        public float initialStrength;
        public float initialDexterity;
        public float initialIntelligence;
        public float initialSpecial; //used for structure abilities
        public float levelUpStrength;
        public float levelUpDexterity;
        public float levelUpIntelligence;
        public float levelUpSpecial;
    }

    public StatBlock statBlock;

    void initializeStatsAndAttributes()
	{
		baseStats = new Dictionary<Stat, float> ();
		baseStats.Add (Stat.Strength, statBlock.initialStrength);
		baseStats.Add (Stat.Intelligence, statBlock.initialIntelligence);
		baseStats.Add (Stat.Dexterity, statBlock.initialDexterity);
		baseStats.Add (Stat.Special, statBlock.initialSpecial);

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
	public int GetStat(Stat stat) //stat mods are added or subtracted
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
		return Mathf.RoundToInt(baseStat);
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

	public float getMaxHP { get { return Mathf.RoundToInt(GetStat(Stat.Strength) * 10f) + GetAttribute(Attribute.MaxHealth); } }
    
    public float getHealthPercentage {  get { return currentHealth / getMaxHP; } }

    //RPG Progression
    public int Gold;
    protected int guildDues;
    public float XP; //make private
    public int Level;
    public int GoldCost;
    public int MaxLevel = 30;
  //  public int[] LevelUpCosts;

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

    //Economy
    public int GoldPerTick = 0;
    public List<BasicItem> ProductsSold;
    public List<EquipmentSlot> EquipmentSlots;
    public List<BasicItem> Potions;
    public float costScaling = 0f;
    
    
    //Building Progression
    public List<BasicUpgrade> AvailableUpgrades;
    public List<BasicUpgrade.ID> ResearchedUpgrades;
    public GameObject StructureVisual;

    public List<LevelUnlock> LevelUnlocks;
    [System.Serializable]
    public class LevelUnlock
    {
        public int cost;
        public List<BasicUpgrade.ID> UpgradesRequired; //used for spawning - should combine this and Structure REquirements to one testing function
        public List<BuildRequirement> StructureRequirements; //used for structures
        public List<BasicUnit> StructureTemplatesUnlocked;
        public GameObject StructureVisualTemplate;

        [System.Serializable]
        public class BuildRequirement
        {
            public bool ShouldExist = true;
            public BasicUnit Structure;
            public int MinLevel = 0;
        }
    }

    //Core Identifications
    public List<Tag> Tags;
    public State currentState;
    
    //Components
    Animator animator;
    NavMeshAgent agent;
    LineRenderer lineRenderer;
    new Renderer renderer;
    Rigidbody rigidBody;

    //Relationships
    //public GameObject myTemplate;
    public string parentObjectName;
    public Team team;
    GameObject Home;
    public GameObject MoveTarget;
    BasicUnit MoveTargetUnit { get { return MoveTarget ? MoveTarget.GetComponent<BasicUnit>() : null; } }
    BasicBounty BountyTarget { get { return MoveTarget ? MoveTarget.GetComponent<BasicBounty>() : null; } }
    BasicBounty LastPickedBounty;
    public Vector3 ExploreTarget;
    public float Loyalty = 0.5f;
    // bool spawnedByStructure = false;

    //Search Radii
    public float huntSearchRadius = 20f;
    float storeSearchRadius = 1000f;
    float deathGiftRadius = 15f;
    public float cryForHelpRadius = 10f;
	public float exploreRadius = 50f;
	public bool tetheredToHome = false;

    //Hiring and Spawning
    [System.Serializable]
    public class UnitSpawner
    {
        public bool hiring;
        float remainingHireTime;

        BasicUnit spawningUnit;
        public bool autoSpawningEnabled = false;
        public bool canBeHired = false;
        public BasicUnit SpawnType;
        public int MaxSpawns = 0;
        public float SpawnCooldown = 3;
        private float RemainingSpawnCooldown;
        public List<BasicUnit> Spawns;

        public void Initialize(BasicUnit spawningUnit)
        {
            this.spawningUnit = spawningUnit;
            hiring = false;
        }
        
        public void Update()
        {
            if (autoSpawningEnabled && Spawns.Count < MaxSpawns)
            {
                RemainingSpawnCooldown -= Time.deltaTime;
                if (RemainingSpawnCooldown <= 0)
                    AutoSpawn();
            }

            if(hiring)
            {
                remainingHireTime -= Time.deltaTime;
                if (remainingHireTime <= 0)
                    FinishHiring();
            }
        }
        
        public bool CanSpawn()
        {
            if (canBeHired && spawningUnit.hiring)
                return false;

            if (SpawnType == null || spawningUnit == null || spawningUnit.Tags.Contains(Tag.Dead))
                return false;

            if (spawningUnit.team != null && !spawningUnit.team.AllowedToBuildUnit(SpawnType))
                return false;

            bool hirePermission = true;
            if(canBeHired)
            {
                if (spawningUnit.AllSpawns.Count() >= spawningUnit.maxHirelings)
                    hirePermission = false;
            }

            return (SpawnType.GetComponent<BasicUnit>().GoldCost <= spawningUnit.TeamGold() && Spawns.Count < MaxSpawns && hirePermission);
        }

        public void StartHiring()
        {
            remainingHireTime = SpawnType.hireTime;
            hiring = true;
            spawningUnit.hiring = true;
            if (spawningUnit.team != null)
                spawningUnit.team.Gold -= SpawnType.ScaledGoldCost();
        }

        public void FinishHiring()
        {
            hiring = false;
            spawningUnit.hiring = false;
            AutoSpawn();
        }

        public void AutoSpawn()
        {
            Spawns.Add(spawningUnit.Spawn(SpawnType.gameObject, spawningUnit.gameObject.transform.position));
            RemainingSpawnCooldown = SpawnCooldown;
        }

        public float HiringPercentage()
        {
            return 1 - (remainingHireTime / SpawnType.hireTime);
        }
    }
    public List<UnitSpawner> Spawners;

    public float hireTime;
    public bool hiring;
    public int maxHirelings;

    public List<BasicUnit> GetHiredHeroes()
    {
        List<BasicUnit> heroes = new List<BasicUnit>();
        foreach(GameObject unitObject in AllSpawns)
        {
            if (unitObject != null && unitObject.GetComponent<BasicUnit>() != null && unitObject.GetComponent<BasicUnit>().HasTag(Tag.Hero))
                heroes.Add(unitObject.GetComponent<BasicUnit>());
        }
        return heroes;
    }


    public BasicUnit Spawn(GameObject template, Vector3 position)
    {
        hiring = false;
        GameObject spawnedObject = (GameObject)Instantiate(template, position, transform.rotation);
        AllSpawns.Add(spawnedObject);
        BasicUnit spawnedUnit = spawnedObject.GetComponent<BasicUnit>();
        spawnedUnit.Home = gameObject;
        spawnedUnit.parentObjectName = template.gameObject.name;
        spawnedUnit.team = team;
        if (team != null)
        {
            team.AddUnit(spawnedUnit);
        }
        return spawnedUnit;
    }
    public List<GameObject> AllSpawns;

    //Occupency
    List<BasicUnit> Occupants;
    BasicUnit StructureOccupied;

    //Timers - make private
    public float corpseDuration = 0;
     float remainingCorpseDuration;
     float remainingDecideTime; //not implemented yet
     float timeSinceLastDamage;
     float remainingShoppingTime;
     float remainingGoldTickTime;

    //animation
    public string AttackAnimation;

    //Other constants
    const float stoppingDistanceMargin = .5f;
    const float sleepRegeneratePercentage = .02f;
    const float fleeHealthPercentage = 0.33f;
    const float goHomeHealthPercentage = 0.8f;
    const float guildTaxRate = 0.3f;
    const float decideTime = 1f;
    const float shoppingTime = 2f;
    const float healingPotionPower = 20f;
    const int maxPotions = 5;
    const float maxHuntingDistance = 30f;
    const float levelUpHealAmount = 0.25f;
    const float goldTickCooldown = 10f;
    // Use this for initialization
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        renderer = GetComponent<Renderer>();
        AllSpawns = new List<GameObject>();
        Tags = Tags ?? new List<Tag>();
        Abilities = Abilities ?? new List<BasicAbility>();
        //UpgradesRequired = UpgradesRequired ?? new List<BasicUpgrade.ID>();
        //StructureRequirements = StructureRequirements ?? new List<BuildRequirement>(); 
        remainingGoldTickTime = goldTickCooldown;

        initializeStatsAndAttributes();

        currentHealth = getMaxHP;
        if (Level > 1)
            XP = Mathf.Pow(Level, 2);

        for (int i = 0; i < AvailableUpgrades.Count; i++)
        {
            {
                AvailableUpgrades[i] = Instantiate(AvailableUpgrades[i]).GetComponent<BasicUpgrade>();
                AvailableUpgrades[i].researcher = this;
                AvailableUpgrades[i].gameObject.transform.SetParent(transform);
            }
        }

        if (AttackAnimation.Length != 0)
            animator.SetBool(AttackAnimation, true);
    }

    void Start () {
        for (int i = 0; i < Abilities.Count; i++)
        {
            {
                Abilities[i] = Instantiate(Abilities[i]).GetComponent<BasicAbility>();
                Abilities[i].gameObject.transform.SetParent(transform);
            }
        }
        

        foreach (UnitSpawner spawner in Spawners)
            spawner.Initialize(this);

        if (team == null && Tags.Contains(Tag.Imperial))
            team = GameManager.Main.Player;

        if(team!=null && !team.HasUnit(this))
            team.AddUnit(this);

        if (Tags.Contains(Tag.Structure)) //Structure Setup
        {
            SetNewState(State.Structure);
            corpseDuration = 0;
        }
        else //Normal Unit Setup
        {
            SetNewState(State.Deciding);
            corpseDuration = 10;
            agent.stoppingDistance = 2;
        }

        GameObject healthBar = Instantiate(GameManager.Main.HealthBarTemplate);
        healthBar.GetComponent<UIHealthBar>().myUnit = this;

        if (name == null || name.Length == 0)
            name = templateID;
    }
	
	// Update is called once per frame
	void Update () {
        timeSinceLastDamage += Time.deltaTime;

        switch (currentState) //This is the only place the Logic functions should be called
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
            case State.Shopping:
                ShoppingLogic();
                break;
            case State.GoingHome:
                GoingHomeLogic();
                break;
            case State.Sleeping:
                SleepLogic();
                break;
            case State.Fleeing:
                FleeingLogic();
                break;
            case State.Dead:
                Die();
                break;
            case State.ExploreBounty:
                ExploreBountyLogic();
                break;

            default:
                break;
        }

        if(!Tags.Contains(Tag.Structure))
        animator.SetBool("Walking", agent.velocity.magnitude > 0.05f);

        foreach (UnitSpawner spawner in Spawners)
            spawner.Update();

        CleanUp();
	}

    void SetNewState(State newState, string reason = "")
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

    bool WithinActivationRange()
    {
        if (currentState == State.Exploring)
            return Vector3.Distance(transform.position, ExploreTarget) < agent.stoppingDistance + stoppingDistanceMargin;
        return isWithinRange(MoveTarget, agent.stoppingDistance + stoppingDistanceMargin);
    }

    void CleanUp()
    {
        remainingGoldTickTime = Mathf.Max(0, remainingGoldTickTime - Time.deltaTime);
        if(remainingGoldTickTime == 0)
        {
            remainingGoldTickTime = goldTickCooldown;
            GainGold(GoldPerTick,false);
        }

        if (currentHealth <= 0 && currentState != State.Dead)
            Die();

        while (AllSpawns.Contains(null))
            AllSpawns.Remove(null);

        foreach(UnitSpawner spawner in Spawners)
        {
            while (spawner.Spawns.Contains(null))
                spawner.Spawns.Remove(null);
        }
        
        //Move Towards Target's new position
        if (agent != null && agent.isActiveAndEnabled)
        {
            if (MoveTarget != null)
                agent.SetDestination(MoveTarget.transform.position);
            else if (ExploreTarget == Vector3.zero)
                agent.SetDestination(transform.position);
        }
    }

    //DECIDING - CENTRAL LOGICAL NEXUS
    void StartDeciding()
    {
        agent.Stop();
        if (Tags.Contains(Tag.Structure))
        {
            SetNewState(State.Structure);
            return;
        }
        SetNewState(State.Deciding);
        remainingDecideTime = decideTime;
    }

    void DecideLogic()
    {
        if (Tags.Contains(Tag.Dead))
        {
            Die();
            return;
        }

        if (Tags.Contains(Tag.Structure))
        {
            SetNewState(State.Structure);
            return;
        }

        remainingDecideTime -= Time.deltaTime;
        if (remainingDecideTime <= 0)
        {
            StopDeciding();

            float RandomSelection = Random.Range(0, 100);

            if (Home != null && ShouldIFlee() && Tags.Contains(Tag.Hero))
                StartFleeing();
            else if (Home != null && RandomSelection < 10 && getHealthPercentage < goHomeHealthPercentage)
                StartGoingHome();
            else if (RandomSelection < 60)
                StartHunting();
            else if (RandomSelection < 80 && Tags.Contains(Tag.Hero) && FindStore())
                StartShopping();
            else if (RandomSelection < 90 && team == GameManager.Main.Player && Tags.Contains(Tag.Hero) && PickBounty())
            {
                if (BountyTarget.type == BasicBounty.Type.Explore)
                    StartExploreBounty();
                else
                    EndBounty();
            }
            else
                StartExploring();
        }

    }

    void StopDeciding()
    {
        if(agent!=null)
            agent.Resume();
    }


    //GOING HOME
    void StartGoingHome() {
        agent.stoppingDistance = 1;
        SetNewState(State.GoingHome);
    }
    void GoingHomeLogic()
    {
        MoveTarget = Home;
        if (MoveTarget == null)
            StopFleeing();
        else if (WithinActivationRange())
        {
            StopFleeing();
            StartSleeping();
        }
    }
    void StopGoingHome() {
        SetNewState(State.Deciding);
    }

    //FLEEING - EXACTLY LIKE GOING HOME EXCEPT CAN'T BE INTERRUPTED
    void StartFleeing()
    {
        SetNewState(State.Fleeing);
    }
    void FleeingLogic()
    {
        MoveTarget = Home;
        if (MoveTarget == null)
            StopFleeing();
        else if (WithinActivationRange())
        {
            StopFleeing();
            StartSleeping();
        }
    }
    void StopFleeing()
    {
        SetNewState(State.Deciding);
    }

    //SLEEPING - WHEN SLEEPING, YOU GRADUALLY HEAL
    void StartSleeping() {
        SetNewState(State.Sleeping);
        Home.GetComponent<BasicUnit>().GainGold(guildDues, false);
        guildDues = 0;
    }
    void SleepLogic() {
        DealHealing(sleepRegeneratePercentage * getMaxHP * Time.deltaTime, this);
        if (currentHealth >= getMaxHP)
            StopSleeping();
    }
    void StopSleeping() {
        SetNewState(State.Deciding);
    }
    
   

    bool HasHealingPotions()
    {
        return Potions.Count > 0;
    }

    bool PickBounty()
    {
        bool success = false;
        if(GameManager.Main.AllBounties.Count > 0)
        {
            LastPickedBounty = GameManager.Main.AllBounties[Random.Range(0, GameManager.Main.AllBounties.Count - 1)];
            MoveTarget = LastPickedBounty.gameObject;
            success = true;
        }
        return success;
    }

    void StartExploreBounty()
    {
        agent.stoppingDistance = 4f;
        SetNewState(State.ExploreBounty);
        MoveTarget = LastPickedBounty.gameObject;
    }

    void ExploreBountyLogic()
    {
        if (BountyTarget == null)
        {
            EndBounty();
            return;
        }

        if (WithinActivationRange())
        {
            BountyTarget.ClaimExploreBounty(this);
            EndBounty();
        }
    }

    void EndBounty()
    {
        SetNewState(State.Deciding);
    }


    //EXPLORING

	void StartExploring()
	{
        SetNewState(State.Exploring);
	}

    void ExploreLogic()
    {
        agent.stoppingDistance = 2;

        if(!agent.pathPending)
        {
            if(ExploreTarget == Vector3.zero || !agent.hasPath)
            {
                RandomNavmeshPoint(transform.position, exploreRadius, out ExploreTarget);
                agent.SetDestination(ExploreTarget);
            }
            else if (WithinActivationRange())
                StopExploring();
        }

    }

    bool RandomNavmeshPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    void StopExploring()
    {
        SetNewState(State.Deciding);
    }
    

    //SHOPPING

    void StartShopping()
    {
        SetNewState(State.Shopping);
    }

    void ShoppingLogic()
    {
        if (MoveTargetUnit == null || !MoveTargetUnit.Tags.Contains(Tag.Store))
        {
            BasicUnit store = FindStore();
            if (!store)
                DoneShopping();
            else
            {
                MoveTarget = store.gameObject;
                remainingShoppingTime = shoppingTime;
            }
        }
        else
        {
            agent.stoppingDistance = 1;
            if (WithinActivationRange())
                BrowseWares();
        }
    }

    void BrowseWares()
    {
        remainingShoppingTime -= Time.deltaTime;
        if (remainingShoppingTime <= 0)
        {
            List<BasicItem> affordableItems = ItemsICanAffordAtStore(MoveTargetUnit);
            if (affordableItems.Count > 0)
                BuyItem(affordableItems[0]);
            DoneShopping();
        }
    }

    List<BasicItem> ItemsICanAffordAtStore(BasicUnit store)
    {
        List<BasicItem> desiredItems = new List<BasicItem>(); 
        int productCount = new List<BasicItem>(store.ProductsSold).Count; //if you put this in the for loop it fucking explodes
        for (int product = 0; product < productCount; product++)
        {
            for (int slot = 0; slot < EquipmentSlots.Count(); slot++)
            {
                BasicItem soldItem = store.ProductsSold[product];
                if (EquipmentSlots[slot].Type == soldItem.Type && soldItem.Cost <= Gold)
                {
                    if (soldItem.Type == BasicItem.ItemType.Potion)
                    {
                        if (Potions.Count() < maxPotions)
                            desiredItems.Add(soldItem);
                    }
                    else if (EquipmentSlots[slot].Instance == null || soldItem.Level > EquipmentSlots[slot].Instance.Level)
                        desiredItems.Add(soldItem);
                }
            }
        }
        return desiredItems;
    }

    void BuyItem(BasicItem soldItem)
    {
        Gold -= soldItem.Cost;
        MoveTargetUnit.GainGold(soldItem.Cost, false);

        if (soldItem.Type == BasicItem.ItemType.Potion)
        {
            BasicItem newPotion = Instantiate(soldItem.gameObject).GetComponent<BasicItem>();
            Potions.Add(newPotion);
            newPotion.gameObject.transform.SetParent(transform);
        }
        else
        {
            for (int i = 0; i < EquipmentSlots.Count(); i++)
            {
                if (EquipmentSlots[i].Type == soldItem.Type)
                {
                    EquipmentSlots[i].Instance = Instantiate(soldItem.gameObject).GetComponent<BasicItem>();
                    EquipmentSlots[i].Instance.gameObject.transform.SetParent(transform);
                    EquipmentSlots[i].Instance.gameObject.transform.localPosition = Vector3.zero;
                    break;
                }
            }
        }
    }

    BasicUnit FindStore()
    {
        List<BasicUnit> initialTargets = GetUnitsWithinRange(storeSearchRadius);
        List<BasicUnit> acceptableTargets = new List<BasicUnit>();
        foreach (BasicUnit encounteredUnit in initialTargets)
            if (encounteredUnit.Tags.Contains(Tag.Store) && encounteredUnit.team == team && ItemsICanAffordAtStore(encounteredUnit).Count > 0)
                acceptableTargets.Add(encounteredUnit);

        if (acceptableTargets.Count > 0)
            return acceptableTargets[Random.Range(0, acceptableTargets.Count - 1)];
        return null;
    }

    void DoneShopping()
    {
        SetNewState(State.Deciding);
    }

    //HUNTING

    void StartHunting(BasicUnit optionalTarget = null)
    {
        SetNewState(State.Hunting);
        if(optionalTarget!=null)
            MoveTarget = optionalTarget.gameObject;
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
            currentAbility = null;
            List<BasicUnit> acceptableTargets = chooseAbilityAndFindPossibleTargets();

            acceptableTargets = sortByDistance(acceptableTargets);

            if (acceptableTargets.Count != 0)
            {
                MoveTarget = acceptableTargets[0].gameObject; //Random.Range(0, acceptableTargets.Count) //pick closest target
            }
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
        SetNewState(State.Deciding);
	}

    List<BasicUnit> sortByDistance(List<BasicUnit> PotentialTargets)
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

    void ChannelAbility() //only activated if cooldown is 0 and target isn't null
    {
        agent.SetDestination(MoveTarget.transform.position);

        BasicUnit initialAbilityTarget = MoveTargetUnit;
		if (currentAbility == null)
			PickAbility();

        if (!currentAbility.isValidTarget(initialAbilityTarget)) {
            if(debugMode)
                Debug.Log("Initial target no longer valid! Cancelling ability use.");
			InterruptAbility ();
			return;
		}

		if (isWithinRange (initialAbilityTarget.gameObject, currentAbility.range)) {
            RotateTowards(initialAbilityTarget.gameObject.transform);
            //Debug.Log ("In range of ability!");
            if (currentAbility.CanCast() && !currentAbility.running) {
                currentAbility.StartCasting (initialAbilityTarget);
                if (agent)
                    agent.Stop();
            }
		}


        if (currentAbility.finished) {
            if (agent)
                agent.Resume();
		}
    }


    public void SetAnimationState(string animationName, bool value)
    {
        if (animator != null)
            animator.SetBool(animationName, value);

    }

    private void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed/100f);
    }

    void InterruptAbility()
    {
        SetAnimationState("Firing", false);
        if (currentAbility != null)
        { // && !currentAbility.finished
            if (debugMode)
			    Debug.Log ("Ability Interrupted");
			currentAbility.FinishAbility();
		}
		currentAbility = null;
        MoveTarget = null;

        if (agent)
            agent.Resume();
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
        timeSinceLastDamage = 0;
      //  if (debugMode)
         //   Debug.Log(gameObject.name + " is taking " + damage + " damage from " + source.gameObject.name);

        currentHealth -= Mathf.Max(0, damage);

        if (currentHealth == 0 && Potions.Count > 0)
            UsePotion();

        if (passiveStates.Contains(currentState))
        {
            if (!Tags.Contains(Tag.Structure))
                StartDeciding();
        }
        else if (currentState == State.Hunting)
        {
            if (Tags.Contains(Tag.Hero) && ShouldIFlee())
            {
                StopHunting();
                StartFleeing();
            }
            else if (MoveTarget != null && (MoveTargetUnit.Tags.Contains(Tag.Structure) || Vector3.Distance(MoveTargetUnit.transform.position, transform.position) > maxHuntingDistance))
                StartHunting(source);
        }


        BroadcastCryForHelp(source);
    }

    public void TakeHealing(float damage, BasicUnit source)
    {
        currentHealth += Mathf.Max(0,damage);
        currentHealth = Mathf.Min(currentHealth, getMaxHP);
    }

    bool ShouldIFlee()
    {
        if (!disabledStates.Contains(currentState))
        {
            while (Potions.Count > 0 && getHealthPercentage < fleeHealthPercentage)
                UsePotion();
        }
        return getHealthPercentage < fleeHealthPercentage;
    }

    public bool isWithinRange(GameObject potentialTarget, float range)
    {
        RaycastHit[] hitInfo = Physics.RaycastAll(transform.position, potentialTarget.transform.position - transform.position, range);
        for (int i = 0; i < hitInfo.Length; i++)
        {
            if (hitInfo[i].collider.gameObject.GetInstanceID() == potentialTarget.gameObject.GetInstanceID())
                return true;
        }
        return false;
    }


    //DEATH

    void Die()
    {
        SetNewState(State.Dead);
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.speed = 0;
            agent.Stop();
            agent.enabled = false;
        }

        NavMeshObstacle obstacle = GetComponent<NavMeshObstacle>();

        if(obstacle!=null)
        {
            obstacle.enabled = false;
        }

        if (!Tags.Contains(Tag.Dead))
        { //then initialize the death state

            Tags.Add(Tag.Dead);
            remainingCorpseDuration = corpseDuration;

            if (Tags.Contains(Tag.Monster))
                OnDeathDistributeGoldAndXP();


            SetAnimationState("Dead", true);

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
                goldRecipients[i].GainGold(Gold, true);
                Gold = 0;
            }
            else
            {
                int goldGranted = initialGold / goldRecipients.Count;
                goldRecipients[i].GainGold(goldGranted,true);
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


    //PROGRESSION

    void GainGold(int goldAmount, bool taxableIncome)
    {
        //add visual and sound effects
        if (Tags.Contains(Tag.Store) && team != null)
        {
            Gold += goldAmount;
            team.Gold += Gold;
            Gold = 0;
        }
        else if (taxableIncome)
        {
            int taxes = Mathf.RoundToInt(guildTaxRate * (float)goldAmount);
            guildDues += taxes;
            Gold += goldAmount - taxes;
        }
        else
            Gold += goldAmount;
    }

    void GainXP(float xpAmount)
    {
       // Debug.Log(gameObject.name + " earned " + xpAmount + " XP");

        XP += xpAmount; 
        int proposedNewLevel = (int)Mathf.Sqrt(XP);
        while (Level < proposedNewLevel)
            LevelUp();
    }

	void LevelUp(bool healOnLevelUp = true) //add visual and sound effects
    {
        Level++;
        if(debugMode)
            Debug.Log(gameObject.name + " leveled up to " + Level + "!");
        //gameObject.name = (parentObjectName.Length ==0 ? gameObject.name : parentObjectName) + " " + Level;

		baseStats[Stat.Strength] += statBlock.levelUpStrength;
		baseStats[Stat.Dexterity] += statBlock.levelUpDexterity;
		baseStats[Stat.Intelligence] += statBlock.levelUpIntelligence;
		baseStats [Stat.Special] += statBlock.levelUpSpecial;

        if (healOnLevelUp)
            TakeHealing(getMaxHP * levelUpHealAmount,this);
            
    }



    
  

    //Crying for Help

    void BroadcastCryForHelp(BasicUnit attacker)
    {
        List<BasicUnit> nearbyUnits = GetUnitsWithinRange(cryForHelpRadius);
        foreach(BasicUnit unit in nearbyUnits)
        {
            if (unit.team == team)
                unit.HearCryForHelp(this, attacker);
        }

        foreach(GameObject unitObject in AllSpawns)
        {
            BasicUnit unit = unitObject.GetComponent<BasicUnit>();
            if (unit.team == team)
                unit.HearCryForHelp(this, attacker);
        }
    }

    protected void HearCryForHelp(BasicUnit unitInDistress, BasicUnit attacker)
    {
        if (!Tags.Contains(Tag.Structure) && attacker != null && unitInDistress != null)
        {
            if (passiveStates.Contains(currentState) && Random.Range(0f, 1f) < Loyalty && !attacker.Tags.Contains(Tag.Dead))
            {
                StartHunting(attacker);
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

    //Upgrades
    public void ResearchUpgrade(BasicUpgrade upgrade)
    {
        upgrade.StartResearch();
    }

    //Structure Leveling Up
    public bool AnotherStructureLevelExists()
    {
        return Level < LevelUnlocks.Count()-1;
    }

    public bool CanAffordToLevelUpStructure()
    {
        return TeamGold() >= LevelUpCost();
    }

    public int LevelUpCost()
    {
        return LevelUnlocks[Level+1].cost;
    }

    public void LevelUpStucture()
    {
        team.Gold -= LevelUpCost();
        LevelUp(false);
        GameManager.Main.PossibleOptionsChange(this);
    }

    //UI Considerations
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Main.StartInspection(this);
    }

    //Other stuff
    bool UsePotion()
    {
        if(Potions.Count>0)
        {
            Debug.Log(gameObject.name + " used healing potion!");
            BasicItem usedPotion = Potions[0];
            DealHealing(healingPotionPower, this);
            Potions.Remove(usedPotion);
            Destroy(usedPotion.gameObject);
            return true;
        }
        return false;
    }
    

    void OnDestroy()
    {
        if (team != null && team.HasUnit(this))
            team.RemoveUnit(this);
    }

    int TeamGold()
    {
        if (team != null)
            return team.Gold;
        else
            return 0;
    }

    public List<BasicUnit> StructuresUnlocked()
    {
        List<BasicUnit> structures = new List<BasicUnit>();
        for (int i = 0; i <= Level && i < LevelUnlocks.Count; i++)
        {
            foreach (BasicUnit newUnlockedStructure in LevelUnlocks[i].StructureTemplatesUnlocked)
                structures.Add(newUnlockedStructure);
            /*foreach (BasicUpgrade.ID upgradeID in LevelUnlocks[i].FreeUpgrades)
                team.TeamUpgrades.Add(upgradeID);*/
        }
        return structures;
    }

    public int ScaledGoldCost()
    {
        return (int)(GoldCost + GoldCost * costScaling * GameManager.Main.Player.GetInstances(templateID).Count);
    }

    public bool HasTag(Tag tag, BasicUnit questioner = null)
    {
        if (questioner != null)
        {
            if (tag == Tag.Self)
                return questioner == this;

            if (tag == Tag.Ally)
                return questioner.team == team && !Tags.Contains(Tag.Neutral);

            if (tag == Tag.Enemy)
                return questioner.team != team && !Tags.Contains(Tag.Neutral);
        }
        return Tags.Contains(tag);
    }

    public void AddTag(Tag tag)
    {
        Tags.Add(tag);
    }
    public void RemoveTag(Tag tag)
    {
        Tags.Remove(tag);
    }
}
