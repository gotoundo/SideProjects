using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;


public class BasicUnit : MonoBehaviour,  IPointerClickHandler{
    public bool debugMode = false;
    
    //Enumerations
    public enum Tag { Structure, Organic, Imperial, Monster, Mechanical, Dead, Store, Self, Hero, Consumed}
    public enum State { None, Deciding, Exploring, Hunting, InCombat, Following, Shopping, GoingHome, Fleeing, Relaxing, Sleeping, Dead, Structure, Stunned, ExploreBounty,KillBounty,DefendBounty } //the probabilities of which state results after "Deciding" is determined per class
	public enum Stat{ Strength, Dexterity, Intelligence, Special}
	public enum Attribute { None, MaxHealth, PhysicalDamage, MagicDamage, MoveSpeed, AttackSpeed, HealthRegen, MaxMana, ManaRegen }

    List<State> passiveStates = new List<State>( new State[] { State.Exploring, State.ExploreBounty, State.Shopping, State.Sleeping, State.GoingHome }); // these can be interrupted


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
    
    //RPG Progression
    public int Gold;
    public int guildDues;
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
    
    public List<BasicItem> ProductsSold;
    public int HealingPotions;

    //Store

    public List<EquipmentSlot> EquipmentSlots;
    public List<BasicUpgrade> AvailableUpgrades;
    public List<BasicUpgrade.ID> ResearchedUpgrades;


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
    BasicBounty BountyTarget { get { return MoveTarget ? MoveTarget.GetComponent<BasicBounty>() : null; } }
    BasicBounty LastPickedBounty;
    public Vector3 ExploreTarget;
   // bool spawnedByStructure = false;

    //Search Radii
    public float huntSearchRadius = 20f;
    float storeSearchRadius = 1000f;
    float deathGiftRadius = 20f;
    public float cryForHelpRadius = 10f;
	public float exploreRadius = 50f;
	public bool tetheredToHome = false;
        
    //Spawning
    public bool autoSpawningEnabled = false;
    public GameObject SpawnType;
    public int MaxSpawns = 0;
    public float SpawnCooldown = 3;
    private float RemainingSpawnCooldown;
    public List<GameObject> Spawns;

    //Timers - make private
    public float corpseDuration = 0;
    public float remainingCorpseDuration;
    public float remainingDecideTime; //not implemented yet
    public float timeSinceLastDamage;
    public float remainingShoppingTime;


    //Other constants
    const float stoppingDistanceMargin = 2;
    const float sleepRegeneratePercentage = .02f;
    const float fleeHealthPercentage = 0.33f;
    const float goHomeHealthPercentage = 0.8f;
    const float respondToCryForHelpChance = 0.5f;
    const float guildTaxRate = 0.3f;
    const float decideTime = 1f;
    const float shoppingTime = 2f;
    const float healingPotionPower = 50f;
    const int maxHealingPotions = 5;
    
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

        for (int i = 0; i < Abilities.Count; i++)
        {
            {
                Abilities[i] = Instantiate(Abilities[i]).GetComponent<BasicAbility>();
                Abilities[i].gameObject.transform.SetParent(transform);
            }
        }
        for (int i = 0; i < AvailableUpgrades.Count; i++)
        {
            {
                AvailableUpgrades[i] = Instantiate(AvailableUpgrades[i]).GetComponent<BasicUpgrade>();
                AvailableUpgrades[i].researcher = this;
                AvailableUpgrades[i].gameObject.transform.SetParent(transform);
            }
        }




        XP = Mathf.Pow(Level,2);

        if (team == null && Tags.Contains(Tag.Imperial))
            team = GameManager.Main.Player;

        if (Tags.Contains(Tag.Structure)) //Structure Setup
        {
            corpseDuration = 0;
            SetNewState(State.Structure);
        }
        else //Normal Unit Setup
        {
            SetNewState(State.Deciding);
            corpseDuration = 15;
            agent.stoppingDistance = 2;
        }

       // lineRenderer.material.color = renderer.material.color;
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

        UpdateSpawning();
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
        //BountyTarget = null;
        currentState = newState;
    }

    bool WithinActivationRange()
    {
        if (currentState == State.Exploring)
            return Vector3.Distance(transform.position, ExploreTarget) < agent.stoppingDistance + stoppingDistanceMargin;
        return Vector3.Distance(transform.position, MoveTarget.transform.position) < agent.stoppingDistance + stoppingDistanceMargin;
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
            
            
            //if (currentState == State.Hunting && currentAbility != null && currentAbility.casting && WithinActivationRange())
               // agent.Stop();

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

            if (Home != null && currentHealth / getMaxHP < fleeHealthPercentage && !HasHealingPotions() && Tags.Contains(Tag.Hero))
                StartFleeing();
            else if (Home != null && RandomSelection < 10 && currentHealth / getMaxHP < goHomeHealthPercentage)
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
        agent.stoppingDistance = 5;
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
        return HealingPotions > 0;
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
            agent.stoppingDistance = 3;
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
                if (EquipmentSlots[slot].Type == soldItem.Type)
                {
                    if (EquipmentSlots[slot].Instance == null || soldItem.Level > EquipmentSlots[slot].Instance.Level)
                    {
                        if (soldItem.Cost <= Gold)
                            desiredItems.Add(soldItem);
                    }
                }
            }
        }
        return desiredItems;
    }

	void BuyItem(BasicItem soldItem)
	{
		Gold -= soldItem.Cost;
		MoveTargetUnit.GainGold(soldItem.Cost, false);
        for (int i = 0; i < EquipmentSlots.Count(); i++)
        {
            if (EquipmentSlots[i].Type == soldItem.Type)
            {
                EquipmentSlots[i].Instance = Instantiate(soldItem.gameObject).GetComponent<BasicItem>();
                EquipmentSlots[i].Instance.gameObject.transform.SetParent(transform);
                EquipmentSlots[i].Instance.gameObject.transform.localPosition = Vector3.zero;
                if (debugMode)
                    Debug.Log(gameObject.name + " bought " + EquipmentSlots[i].Instance.name + " from " + MoveTarget + ".");
                break;
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
    

    int TeamGold()
    {
        if (team != null)
            return team.Gold;
        else
            return 0;
    }

    public bool CanSpawn()
    {
        return (SpawnType != null && SpawnType.GetComponent<BasicUnit>().GoldCost <= TeamGold() && Spawns.Count < MaxSpawns);
    }

    public BasicUnit Spawn(GameObject template, Vector3 position)
    {
        RemainingSpawnCooldown = SpawnCooldown;

        GameObject spawnedObject = (GameObject)Instantiate(template, position, transform.rotation);
        Spawns.Add(spawnedObject);
        BasicUnit spawnedUnit = spawnedObject.GetComponent<BasicUnit>();
        spawnedUnit.myTemplate = template;
        spawnedUnit.Home = gameObject;
        //spawnedUnit.spawnedByStructure = true;
        spawnedUnit.parentObjectName = template.gameObject.name;
        spawnedUnit.team = team;
        spawnedObject.name = template.name;
        if(team!=null)
            team.Gold -= spawnedUnit.GoldCost;

        return spawnedUnit;
    }


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
            List<BasicUnit> acceptableTargets = chooseAbilityAndFindPossibleTargets();

            acceptableTargets = sortByDistance(acceptableTargets);

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

    void ChannelAbility() //only activated if cooldown is 0
    {
        agent.SetDestination(MoveTarget.transform.position);

        BasicUnit initialAbilityTarget = MoveTargetUnit;
		if (currentAbility == null)
			PickAbility();

        if (!currentAbility.isValidTarget(initialAbilityTarget)) {
			//Debug.Log("Initial target no longer valid! Cancelling ability use.");
			InterruptAbility ();
			return;
		}

		if (currentAbility.isWithinRange (initialAbilityTarget)) {
            RotateTowards(initialAbilityTarget.gameObject.transform);
            //Debug.Log ("In range of ability!");
            if (currentAbility.CanCast() && !currentAbility.running) {

                currentAbility.StartCasting (initialAbilityTarget);
                if (agent)
                    agent.Stop();
            }
		}

        animator.SetBool("Firing",  true);

        if (currentAbility.finished) {
            if (agent)
                agent.Resume();
		}
    }

    private void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed);
    }

    void InterruptAbility()
    {
        animator.SetBool("Firing", false);
        if (currentAbility != null && !currentAbility.finished) {
            if(debugMode)
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
        if (debugMode)
            Debug.Log(gameObject.name + " is taking " + damage + " damage from " + source.gameObject.name);

        currentHealth -= Mathf.Max(0, damage);

        if (passiveStates.Contains(currentState))
        {
            if (!Tags.Contains(Tag.Structure))
                StartDeciding();
        }
        else if (currentState == State.Hunting)
        {
            if (Tags.Contains(Tag.Hero) && currentHealth / getMaxHP < fleeHealthPercentage)
            {
                StopHunting();
                StartFleeing();
            }
            else if (MoveTarget != null && MoveTargetUnit.Tags.Contains(Tag.Structure))
                StartHunting(source);
        }


        BroadcastCryForHelp(source);
    }

    public void TakeHealing(float damage, BasicUnit source)
    {
        currentHealth += Mathf.Max(0,damage);
        currentHealth = Mathf.Min(currentHealth, getMaxHP);
    }

    void Die()
    {
        SetNewState(State.Dead);
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

	void LevelUp() //add visual and sound effects
    {
        Level++;
        if(debugMode)
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

        foreach(GameObject unitObject in Spawns)
        {
            BasicUnit unit = unitObject.GetComponent<BasicUnit>();
            if (unit.team == team)
                unit.HearCryForHelp(this, attacker);
        }
    }

    public void HearCryForHelp(BasicUnit unitInDistress, BasicUnit attacker)
    {
        if (!Tags.Contains(Tag.Structure) && attacker != null && unitInDistress != null)
        {
            if (passiveStates.Contains(currentState) && Random.Range(0f, 1f) < respondToCryForHelpChance && !attacker.Tags.Contains(Tag.Dead))
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

    //UI Considerations
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Main.StartInspection(this);
    }
}
