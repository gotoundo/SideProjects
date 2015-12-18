using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;


public class BasicUnit : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler
{
    public bool debugMode = false;

    public string templateID;
    public string templateDescription;
    new public string name;
    public DataNameBank nameBank;
   
    //Enumerations
    public enum Tag { Structure, Organic, Imperial, Monster, Mechanical, Dead, Store, Self, Hero, Consumed, Enemy, Ally, Neutral, Inside, Hidden }
    public enum State { None, Deciding, Exploring, Hunting, InCombat, Following, GoingShopping, GoingHome, Fleeing, Relaxing, Sleeping, Dead, Structure, Stunned, ExploreBounty, KillBounty, DefendBounty, Browsing } //the probabilities of which state results after "Deciding" is determined per class
    public enum Stat { Strength, Dexterity, Intelligence, Special, Sensitivity }
    public enum Attribute { None, MaxHealth, KineticDamage, EnergyDamage, MoveSpeed, AttackSpeed, HealthRegen, MaxEnergy, ManaRegen, Armor, PsychicDamage }


    List<State> combatStates = new List<State>(new State[] { State.Hunting,State.KillBounty }); //
    List<State> passiveStates = new List<State>(new State[] { State.Exploring, State.ExploreBounty, State.GoingShopping, State.GoingHome }); // these can be interrupted by combat
    List<State> disabledStates = new List<State>(new State[] { State.Stunned }); 

    //Attributes and Stats
    Dictionary<Attribute, float> baseAttributes;
    Dictionary<Stat, float> baseStats;

    [System.Serializable]
    public class StatBlock
    {
        public float initialStrength;
        public float initialDexterity;
        public float initialIntelligence;
        public float initialSpecial; //used for structure and ally abilities
        public float initialSensitivity;
        public float levelUpStrength;
        public float levelUpDexterity;
        public float levelUpIntelligence;
        public float levelUpSpecial;
        public float levelUpSensitivity;
    }

    public StatBlock statBlock;
    public bool usesEnergy = false;

    void initializeStatsAndAttributes()
    {
        baseStats = new Dictionary<Stat, float>();
        baseStats.Add(Stat.Strength, statBlock.initialStrength);
        baseStats.Add(Stat.Intelligence, statBlock.initialIntelligence);
        baseStats.Add(Stat.Dexterity, statBlock.initialDexterity);
        baseStats.Add(Stat.Special, statBlock.initialSpecial);
        baseStats.Add(Stat.Sensitivity, statBlock.initialSensitivity);

        baseAttributes = new Dictionary<Attribute, float>();
        baseAttributes.Add(Attribute.AttackSpeed, 0);
        baseAttributes.Add(Attribute.HealthRegen, 0);
        baseAttributes.Add(Attribute.EnergyDamage, 0);
        baseAttributes.Add(Attribute.ManaRegen, 0);
        baseAttributes.Add(Attribute.MaxHealth, 0);
        baseAttributes.Add(Attribute.MaxEnergy, 0);
        baseAttributes.Add(Attribute.MoveSpeed, agent ? agent.speed : 0);
        baseAttributes.Add(Attribute.None, 0);
        baseAttributes.Add(Attribute.KineticDamage, 0);
        baseAttributes.Add(Attribute.Armor, 0);
        baseAttributes.Add(Attribute.PsychicDamage, 0);
    }

    public int GetStat(Stat stat) //stat mods are added or subtracted
    {
        float baseStat = baseStats[stat];

        foreach (BasicItem item in AllItems)
        {
            if (item != null)
            {
                for (int j = 0; j < item.StatEffects.Count(); j++)
                    if (item.StatEffects[j].stat == stat)
                        baseStat += item.StatEffects[j].value + item.EnchantmentLevel;
            }
        }

        foreach (BasicBuff buff in attachedBuffs)
        {
            if (buff != null)
            {
                for (int j = 0; j < buff.StatEffects.Count(); j++)
                    if (buff.StatEffects[j].stat == stat)
                        baseStat += buff.StatEffects[j].value;
            }
        }

        return Mathf.Max(0,Mathf.RoundToInt(baseStat));
    }

    public float GetAttribute(Attribute attribute) //attribute modifiers are added or subtracted
    {
        float baseAttribute = baseAttributes[attribute];

        foreach (BasicItem item in AllItems)
        {
            if (item != null)
            {
                for (int j = 0; j < item.AttributeEffects.Count(); j++)
                    if (item.AttributeEffects[j].attribute == attribute)
                        baseAttribute += item.AttributeEffects[j].value + item.EnchantmentLevel;
            }
        }

        foreach (BasicBuff buff in attachedBuffs)
        {
            if (buff != null)
            {
                for (int j = 0; j < buff.AttributeEffects.Count(); j++)
                    if (buff.AttributeEffects[j].attribute == attribute)
                        baseAttribute += buff.AttributeEffects[j].value;
            }
        }

        return Mathf.Max(0,baseAttribute);
    }
    
    //Derived Statistics
    public float getMaxEnergy { get { return Mathf.RoundToInt(GetStat(Stat.Intelligence) * 5f) + GetAttribute(Attribute.MaxEnergy); } }
    public float getEnergyRegen { get { return Mathf.RoundToInt(GetStat(Stat.Intelligence) * .5f); } }
    public float getMaxHealth { get { return Mathf.RoundToInt(GetStat(Stat.Strength) * 10f) + GetAttribute(Attribute.MaxHealth); } }

    public float getResistKenetic { get { return Mathf.RoundToInt(GetAttribute(Attribute.Armor)); } }
    public float getResistEnergy { get { return Mathf.RoundToInt(GetStat(Stat.Dexterity)/2 + GetStat(Stat.Intelligence)/2); } }
    public float getResistPsychic { get { return Mathf.RoundToInt(GetStat(Stat.Intelligence)/2 + GetStat(Stat.Sensitivity)/2); } }
    
    public float getHealthPercentage { get { return currentHealth / getMaxHealth; } }

    //Personality Features, as percentages
    public float Loyalty = 0.5f;
    public float Greed = 0.5f;
    public float Temperence = 0.5f;

    //RPG Progression
    public int Gold;
    protected int guildDues;
    public float XP;
    public int Level;
    public int GoldCost;
    public int MaxLevel = 30;

    //RPG Combat
    public float currentEnergy;
    public float currentHealth;
    public List<BasicAbility> Abilities;
    BasicAbility currentAbility;
    public List<BasicBuff> attachedBuffs;

    [System.Serializable]
    public class EquipmentSlot
    {
        public BasicItem.ItemType Type;
        public BasicItem Instance;
    }

    //Economy
    public int GoldPerTick = 0; //workaround doesn't work - units should contribute this to team if they're not structures
    public List<BasicItem> ProductsSold;
    public List<BasicAbility> AbilitiesSold;
    public List<EquipmentSlot> EquipmentSlots;
    public List<BasicItem> UniqueItems;
    public List<BasicItem> Potions;
    public float costScaling = 0f;
    public float researchTimeReduction = 0f;
    public float researchCostReduction = 0f;
    public float structureCostReduction = 0f;

    public List<BasicItem> AllItems
    {
        get
        {
            List<BasicItem> items = new List<BasicItem>();
            items.AddRange(UniqueItems);
            //items.AddRange(Potions);
            foreach (EquipmentSlot slot in EquipmentSlots)
                items.Add(slot.Instance);
            return items;
        }
    }


    public List<BasicItem.Enchantment> ItemEnchantmentsSold;



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
    public BasicMotivations Motivations;

    //Components
    Animator animator;
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
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
    BasicBounty BountyTarget;
    Vector3 ExploreTarget;
    
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
        public float InitialCooldown = 0;
        private float RemainingSpawnCooldown;
        public List<BasicUnit> Spawns;

        public void Initialize(BasicUnit spawningUnit)
        {
            this.spawningUnit = spawningUnit;
            RemainingSpawnCooldown = InitialCooldown;
            hiring = false;
        }

        public void Update()
        {
            if (GameManager.Running)
            {
                if (autoSpawningEnabled && Spawns.Count < MaxSpawns)
                {
                    RemainingSpawnCooldown -= Time.deltaTime;
                    if (RemainingSpawnCooldown <= 0)
                        AutoSpawn();
                }

                if (hiring)
                {
                    remainingHireTime -= Time.deltaTime;
                    if (remainingHireTime <= 0)
                        FinishHiring();
                }
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
            if (canBeHired)
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
            GameManager.Main.PossibleOptionsChange(spawningUnit);
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
    bool hiring;
    public int maxHirelings;

    public List<BasicUnit> GetHiredHeroes()
    {
        List<BasicUnit> heroes = new List<BasicUnit>();
        foreach (GameObject unitObject in AllSpawns)
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
    BasicUnit structureOccupied;

    //Timers - make private
    public float corpseDuration = 0;
    float remainingCorpseDuration;
    const float decideTime = 1f;
    float remainingDecideTime;
    const float shoppingTime = 2f;
    float remainingShoppingTime;
    const float exploringTime = 6f;
    public float remainingExploringTime;
    const float goldTickCooldown = 10f;
    float remainingGoldTickTime;
    float timeSinceLastDamage;

    //animation
    public string AttackAnimation;
    public EffectsProfile effectsProfile;
    [System.Serializable]
    public class EffectsProfile
    {
        public ParticleSystem HitVFX; //blood
        public ParticleSystem DeathVFX;
    }

    //Other constants
    const float sleepRegeneratePercentage = .02f;
    const float fleeHealthPercentage = 0.33f;
    const float goHomeHealthPercentage = 0.8f;
    const float guildTaxRate = 0.3f;
    const float healingPotionPower = 20f;
    const int maxPotions = 5;
    const float maxHuntingDistance = 30f;
    const float levelUpHealAmount = 0.25f;
    const float normalUnitCorpseDuration = 10f;
    const float runningSpeedThreshhold = 0.05f;
    const float stoppingDistanceMargin = 0.5f;
    const float defaultStoppingDistance = 2f;
    const float enterBuildingStoppingDistance = 1f;
    const float exploreStoppingDistance = 2f;
    const float startingStructureHealthPercent = 0.05f;

    // Use this for initialization
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        renderer = GetComponent<Renderer>();
        AllSpawns = new List<GameObject>();
        ProductsSold = ProductsSold ?? new List<BasicItem>();
        Tags = Tags ?? new List<Tag>();
        Abilities = Abilities ?? new List<BasicAbility>();
        attachedBuffs = new List<BasicBuff>();
        ItemEnchantmentsSold = ItemEnchantmentsSold ?? new List<BasicItem.Enchantment>();
        UniqueItems = UniqueItems ?? new List<BasicItem>();
        Occupants = Occupants ?? new List<BasicUnit>();
        //UpgradesRequired = UpgradesRequired ?? new List<BasicUpgrade.ID>();
        //StructureRequirements = StructureRequirements ?? new List<BuildRequirement>(); 
        remainingGoldTickTime = goldTickCooldown;

        initializeStatsAndAttributes();

        currentHealth = getMaxHealth;
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

    void Start()
    {
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

        if (team != null && !team.HasUnit(this))
            team.AddUnit(this);

        if (team != null)
        {
            team.structureCostMultiplier -= structureCostReduction;
            team.researchTimeMultiplier -= researchTimeReduction;
            team.researchCostMultiplier -= researchCostReduction;
        }

        if (Tags.Contains(Tag.Structure)) //Structure Setup
        {
            SetNewState(State.Structure);
            corpseDuration = 0;
        }
        else //Normal Unit Setup
        {
            SetNewState(State.Deciding);
            corpseDuration = normalUnitCorpseDuration;
            agent.stoppingDistance = defaultStoppingDistance;
        }

        if (!HasTag(Tag.Hidden))
        {
            GameObject healthBar = Instantiate(GameManager.Main.HealthBarTemplate);
            healthBar.GetComponent<UIHealthBar>().Initialize(this);
        }

        if (name == null || name.Length == 0)
            name = templateID;
        if (nameBank != null)
            name = nameBank.GetRandomName();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Running)
        {
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
                    CombatLogic();
                    break;
                case State.GoingShopping:
                    ShoppingLogic();
                    break;
                case State.Browsing:
                    BrowsingLogic();
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
                case State.KillBounty:
                    KillBountyLogic();
                    break;
                case State.DefendBounty:
                    DefendBountyLogic();
                    break;
                case State.Stunned:
                    //nothing I guess?
                    break;
                default:
                    break;
            }

            if (!Tags.Contains(Tag.Structure))
                animator.SetBool("Walking", agent.velocity.magnitude > runningSpeedThreshhold);

            foreach (UnitSpawner spawner in Spawners)
                spawner.Update();

            CleanUp();
        }
    }

    void SetNewState(State newState, string reason = "", bool clearTargets = true)
    {
        if (debugMode)
            Debug.Log(((int)GameManager.Main.playTime) + " | " + gameObject.name + " started " + newState.ToString() + ": " + reason);

        if (clearTargets)
        {
            MoveTarget = null;
            ExploreTarget = Vector3.zero;
        }

        if (currentAbility != null)
            InterruptAbility();

        currentAbility = null;
        currentState = newState;
    }

    bool WithinActivationRange()
    {
        if (currentState == State.Exploring)
        {
            if (ExploreTarget == null)
                return false;
            return Vector3.Distance(transform.position, ExploreTarget) < agent.stoppingDistance + stoppingDistanceMargin;
        }

        if (MoveTarget == null)
            return false;
        return IsWithinRange(MoveTarget, agent.stoppingDistance + stoppingDistanceMargin);
    }

    void CleanUp()
    {
        remainingGoldTickTime = Mathf.Max(0, remainingGoldTickTime - Time.deltaTime);
        if (remainingGoldTickTime == 0)
        {
            remainingGoldTickTime = goldTickCooldown;
            GainGold(GoldPerTick, false);
        }

        if (currentHealth <= 0 && currentState != State.Dead)
            Die();

        while (AllSpawns.Contains(null))
            AllSpawns.Remove(null);

        foreach (UnitSpawner spawner in Spawners)
        {
            while (spawner.Spawns.Contains(null))
            {
                spawner.Spawns.Remove(null);
                GameManager.Main.PossibleOptionsChange(this);
            }
        }

        //Move Towards Target's new position
        if (agent != null && agent.isActiveAndEnabled && !HasTag(Tag.Inside))
        {
            agent.speed = GetAttribute(Attribute.MoveSpeed);

            if (currentAbility != null && currentAbility.Running)
                agent.SetDestination(transform.position);
            else if (MoveTarget != null)
                agent.SetDestination(MoveTarget.transform.position);
            else if (ExploreTarget == Vector3.zero)
                agent.SetDestination(transform.position);
        }

        if (!HasTag(Tag.Dead) && usesEnergy)
        {
            GainEnergy(getEnergyRegen * Time.deltaTime);
        }

        while (Occupants.Contains(null))
            Occupants.Remove(null);

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

            State proposedState = Motivations.CalculateState(this);

            switch (proposedState)
            {
                case State.None:
                    break;
                case State.Deciding:
                    break;
                case State.Exploring:
                    StartExploring();
                    break;
                case State.Hunting:
                    StartHunting();
                    break;
                case State.InCombat:
                    break;
                case State.Following:
                    break;
                case State.GoingShopping:
                    if (FindStore())
                        StartShopping();
                    break;
                case State.GoingHome:
                    StartGoingHome();
                    break;
                case State.Fleeing:
                    StartFleeing();
                    break;
                case State.Relaxing:
                    break;
                case State.Sleeping:
                    break;
                case State.Dead:
                    break;
                case State.Structure:
                    break;
                case State.Stunned:
                    break;
                case State.ExploreBounty:
                    if (FindBounty(BasicBounty.Type.Explore))
                        StartExploreBounty();
                    else
                        EndBounty();
                    break;
                case State.KillBounty:
                    if (FindBounty(BasicBounty.Type.Kill))
                    {
                        Debug.Log("Kill bounty found!");
                        StartKillBounty();
                    }
                    else
                    {
                        Debug.Log("No bounty found...");
                        EndBounty();
                    }
                    break;
                case State.DefendBounty:
                    if (FindBounty(BasicBounty.Type.Defend))
                        StartDefendBounty();
                    else
                        EndBounty();
                    break;
                case State.Browsing:
                    break;
                default:
                    break;
            }

            /*float RandomSelection = Random.Range(0, 100);

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
                StartExploring();*/
        }

    }

    void StopDeciding()
    {
        if (agent != null)
            agent.Resume();
    }


    //DEBUFF - STUN

    public void StartStun()
    {
        SetNewState(State.Stunned);
    }

    public void EndStun()
    {
        StartDeciding();
    }


    //GOING HOME
    void StartGoingHome()
    {
        agent.stoppingDistance = enterBuildingStoppingDistance;
        SetNewState(State.GoingHome);
    }
    void GoingHomeLogic()
    {
        MoveTarget = Home;
        //GameObject sleepingLocation = MoveTarget;
        if (MoveTarget == null)
            StopGoingHome();
        else if (WithinActivationRange())
        {
            StartSleeping(MoveTarget);
        }
    }
    void StopGoingHome()
    {
        SetNewState(State.Deciding);
    }

    //FLEEING - EXACTLY LIKE GOING HOME EXCEPT CAN'T BE INTERRUPTED
    void StartFleeing()
    {
        agent.stoppingDistance = enterBuildingStoppingDistance;
        SetNewState(State.Fleeing);
    }
    void FleeingLogic()
    {
        MoveTarget = Home;
        GameObject sleepLocation = MoveTarget;
        if (MoveTarget == null)
            StopFleeing();
        else if (WithinActivationRange())
        {
            StopFleeing();
            StartSleeping(sleepLocation);
        }
    }
    void StopFleeing()
    {
        SetNewState(State.Deciding, "Got to safety",false);
    }

    //SLEEPING - WHEN SLEEPING, YOU GRADUALLY HEAL
    void StartSleeping(GameObject sleepLocation = null)
    {
        if (sleepLocation != null)
        {
            EnterStructure(sleepLocation.GetComponent<BasicUnit>());
            if (Home != null && sleepLocation.GetInstanceID() == Home.GetInstanceID())
            {
                Home.GetComponent<BasicUnit>().GainGold(guildDues, false);
                guildDues = 0;
            }
        }
        SetNewState(State.Sleeping);
    }
    void SleepLogic()
    {
        DealHealing(sleepRegeneratePercentage * getMaxHealth * Time.deltaTime, this);
        if (currentHealth >= getMaxHealth)
            StopSleeping();
    }
    void StopSleeping()
    {
        if (structureOccupied != null)
            LeaveStructure();

        SetNewState(State.Deciding);
    }



    bool HasHealingPotions()
    {
        return Potions.Count > 0;
    }


    //BOUNTIES
    bool FindBounty(BasicBounty.Type bountyType)
    {
        List<BasicBounty> acceptableBounties = new List<BasicBounty>();
        foreach (BasicBounty bountyCandidate in GameManager.Main.AllBounties)
            if (bountyCandidate.type == bountyType)
                acceptableBounties.Add(bountyCandidate);

        bool success = false;
        if (acceptableBounties.Count > 0)
        {
            BountyTarget = acceptableBounties[Random.Range(0, acceptableBounties.Count - 1)];
            MoveTarget = BountyTarget.gameObject;
            success = true;
        }
        return success;
    }

    void EndBounty()
    {
        BountyTarget = null;
        SetNewState(State.Deciding);
    }

    //EXPLORE BOUNTY
    void StartExploreBounty()
    {
        agent.stoppingDistance = 4f;
        SetNewState(State.ExploreBounty);
        MoveTarget = BountyTarget.gameObject;
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

    

    //Kill Bounty

    void StartKillBounty()
    {
        agent.stoppingDistance = 4f;
        SetNewState(State.KillBounty);
        MoveTarget = BountyTarget.targetUnit.gameObject;
    }

    void KillBountyLogic()
    {
        if (BountyTarget == null)
        {
            //Debug.Log("BountyTarget is null, ending kill bounty mode");
            EndBounty();
            return;
        }

        //Debug.Log("Entering KillBounty Combat Mode");
        CombatLogic();
    }


    //Defend Bounty

    void StartDefendBounty()
    {
        agent.stoppingDistance = 4f;
        SetNewState(State.DefendBounty);
        MoveTarget = BountyTarget.targetUnit.gameObject;
    }

    void DefendBountyLogic()
    {
        if (BountyTarget == null)
        {
            EndBounty();
            return;
        }
    }


    //EXPLORING

    void StartExploring()
    {
        SetNewState(State.Exploring);
        remainingExploringTime = exploringTime;
    }

    void ExploreLogic()
    {
        agent.stoppingDistance = exploreStoppingDistance;

        

        if (!agent.pathPending)
        {
            if (ExploreTarget == Vector3.zero || !agent.hasPath)
            {
                if(tetheredToHome && Home!=null)
                    RandomNavmeshPoint(Home.transform.position, exploreRadius, out ExploreTarget);
                else
                    RandomNavmeshPoint(transform.position, exploreRadius, out ExploreTarget);

                agent.SetDestination(ExploreTarget);
            }
            else if (WithinActivationRange())
                StopExploring();
        }

        remainingExploringTime -= Time.deltaTime;
        if (remainingExploringTime <= 0)
            StopExploring();

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
        SetNewState(State.GoingShopping);
    }

    void ShoppingLogic()
    {
        if (MoveTargetUnit == null || !MoveTargetUnit.Tags.Contains(Tag.Store))
        {
            BasicUnit store = FindStore();
            if (!store)
                DoneShopping();
            else
                MoveTarget = store.gameObject;
        }
        else
        {
            agent.stoppingDistance = enterBuildingStoppingDistance;
            if (WithinActivationRange())
                StartBrowsing();
        }
    }

    //Browsing
    void StartBrowsing()
    {
        EnterStructure(MoveTargetUnit);
        SetNewState(State.Browsing, "Reached shop", false);
        remainingShoppingTime = shoppingTime;
    }

    void BrowsingLogic()
    {
        remainingShoppingTime -= Time.deltaTime;
        if (remainingShoppingTime <= 0)
        {
            List<BasicItem> affordableItems = ItemsICanAffordAtStore(MoveTargetUnit);
            List<BasicItem.Enchantment> affordableEnchantments = EnchantmentsICanAffordAtStore(MoveTargetUnit);
            List<BasicAbility> affordableAbilities = AbilitiesICanAffordAtStore(MoveTargetUnit);
            if (affordableItems.Count > 0)
                BuyItem(affordableItems[0]);
            if (affordableEnchantments.Count > 0)
                BuyEnchantment(affordableEnchantments[0]);
            if (affordableAbilities.Count > 0)
                BuyAbility(affordableAbilities[0]);
            StopBrowsing();
        }
    }

    void StopBrowsing()
    {
        LeaveStructure();
        SetNewState(State.Deciding);
    }

    //Shopping and Browsing Utilities

    BasicUnit FindStore()
    {
        List<BasicUnit> initialTargets = GetUnitsWithinRange(storeSearchRadius);
        List<BasicUnit> acceptableTargets = new List<BasicUnit>();
        foreach (BasicUnit encounteredUnit in initialTargets)
            if (encounteredUnit.Tags.Contains(Tag.Store) && encounteredUnit.team == team)
                if (ItemsICanAffordAtStore(encounteredUnit).Count > 0 || EnchantmentsICanAffordAtStore(encounteredUnit).Count > 0 || AbilitiesICanAffordAtStore(encounteredUnit).Count>0)
                    acceptableTargets.Add(encounteredUnit);

        if (acceptableTargets.Count > 0)
            return acceptableTargets[Random.Range(0, acceptableTargets.Count - 1)];
        return null;
    }

    List<BasicItem> ItemsICanAffordAtStore(BasicUnit store)
    {
        if (store == null)
            Debug.Log("Accessing null store!");
        List<BasicItem> desiredItems = new List<BasicItem>();
        int productCount = store.ProductsSold.Count; //if you put this in the for loop it fucking explodes
        for (int product = 0; product < productCount; product++)
        {
            BasicItem soldItem = store.ProductsSold[product];
            if (soldItem.Cost <= Gold)
            {
                if (soldItem.Type == BasicItem.ItemType.UnslottedUnique)
                {
                    bool haveOneAlready = false;
                    foreach (BasicItem item in UniqueItems)
                        if (item.name.Equals(soldItem.name))
                            haveOneAlready = true;
                    if (!haveOneAlready)
                        desiredItems.Add(soldItem);
                }
                else
                {
                    for (int slot = 0; slot < EquipmentSlots.Count(); slot++)
                    {
                        if (EquipmentSlots[slot].Type == soldItem.Type)
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
            }
        }
        return desiredItems;
    }

    List<BasicItem.Enchantment> EnchantmentsICanAffordAtStore(BasicUnit store)
    {
        List<BasicItem.Enchantment> AffordableEnchantments = new List<BasicItem.Enchantment>();
        foreach (BasicItem.Enchantment enchantment in store.ItemEnchantmentsSold)
        {
            if (enchantment.EnchantCost <= Gold)
            {
                foreach (EquipmentSlot slot in EquipmentSlots)
                {
                    if (enchantment.GetEnchantTypes().Contains(slot.Type) && slot.Instance != null && slot.Instance.EnchantmentLevel < enchantment.EnchantLevel)
                        AffordableEnchantments.Add(enchantment);
                }
            }
        }
        return AffordableEnchantments;
    }

    List<BasicAbility> AbilitiesICanAffordAtStore(BasicUnit store)
    {
        List<BasicAbility> AffordableAbilities = new List<BasicAbility>();
        foreach (BasicAbility possibileAbility in store.AbilitiesSold)
        {
            bool alreadyKnowAbility = false;
            foreach (BasicAbility knownAbility in Abilities)
                if (knownAbility.name == possibileAbility.name)
                    alreadyKnowAbility = true;

            if (!alreadyKnowAbility && possibileAbility.learnCost <= Gold && possibileAbility.CanBeLearnedBy(this))
            {
                AffordableAbilities.Add(possibileAbility);
            }
        }
        return AffordableAbilities;
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
        else if (soldItem.Type == BasicItem.ItemType.UnslottedUnique)
        {
            BasicItem newItem = Instantiate(soldItem.gameObject).GetComponent<BasicItem>();
            UniqueItems.Add(newItem);
            newItem.gameObject.transform.SetParent(transform);
        }
        else
        {
            for (int i = 0; i < EquipmentSlots.Count(); i++)
            {
                if (EquipmentSlots[i].Type == soldItem.Type)
                {
                    EquipmentSlots[i].Instance = ((GameObject)Instantiate(soldItem.gameObject, transform.position, transform.rotation)).GetComponent<BasicItem>();
                    EquipmentSlots[i].Instance.gameObject.transform.SetParent(transform);
                    break;
                }
            }
        }
    }

    void BuyAbility(BasicAbility soldAbility)
    {
        Gold -= soldAbility.learnCost;
        MoveTargetUnit.GainGold(soldAbility.learnCost, false);
        BasicAbility abilityInstance = Instantiate(soldAbility.gameObject).GetComponent<BasicAbility>();
        Abilities.Add(abilityInstance);
        Debug.Log(name + " learned " + abilityInstance.name + "!");
    }

    void BuyEnchantment(BasicItem.Enchantment enchantment)
    {
        Gold -= enchantment.EnchantCost;
        MoveTargetUnit.GainGold(enchantment.EnchantCost, false);

        foreach (EquipmentSlot slot in EquipmentSlots)
        {
            if (enchantment.GetEnchantTypes().Contains(slot.Type) && slot.Instance != null)
            {
                if (enchantment.EnchantLevel > slot.Instance.EnchantmentLevel)
                {
                    slot.Instance.EnchantmentLevel = enchantment.EnchantLevel;
                    Debug.Log(name + " purchased a +" + enchantment.EnchantLevel + " " + slot.Type.ToString() + " enchantment at " + MoveTargetUnit.name + "!");
                }
            }
        }
    }






    void DoneShopping()
    {
        SetNewState(State.Deciding);
    }

    //HUNTING

    void StartHunting(BasicUnit optionalTarget = null)
    {
        SetNewState(State.Hunting);
        if (optionalTarget != null)
            MoveTarget = optionalTarget.gameObject;
    }

    //Combat - not a standalone state, but this logic is used in states that require fighting
    void CombatLogic()
    {
        if (Abilities.Count == 0)
        {
            if (debugMode)
                Debug.Log("Stopping combat because I have no abilities");
            StopCombat();
            return;
        }

        if (MoveTarget == null) //find unit to focus on
        {
            currentAbility = null;
            List<BasicUnit> acceptableTargets = ChooseAbilityAndFindPossibleTargets();

            if (acceptableTargets.Count != 0)
            {
                MoveTarget = sortByDistance(acceptableTargets)[0].gameObject; //pick closest target
            }
        }

        if (MoveTarget != null)
            ChannelAbility();
        else
        {
            if (debugMode)
                Debug.Log("Stopping Hunting because Move Target is null");
            StopCombat();
        }
    }

    void StopCombat()
    {
        InterruptAbility();
        SetNewState(State.Deciding);
    }

    List<BasicUnit> sortByDistance(List<BasicUnit> PotentialTargets)
    {
        return PotentialTargets.OrderBy(o => Vector3.Distance(o.transform.position, transform.position)).ToList();
    }


    public void DealDamage(float damage, BasicUnit Target, BasicBuff.Effect effect)
    {
        Target.TakeDamage(damage, this,effect);
    }

    public void DealHealing(float healing, BasicUnit Target)
    {
        Target.TakeHealing(healing, this);
    }

    void PickAbility(BasicAbility pickedAbility)
    {
        currentAbility = pickedAbility;
        currentAbility.ResetAbility();
        agent.stoppingDistance = currentAbility.range - stoppingDistanceMargin;
    }

    bool PickAbility()
    {
        List<BasicAbility> CastableAbilitiesByLevel = CastableAbilities();
        if (CastableAbilitiesByLevel.Count > 0)
        {
            PickAbility(CastableAbilitiesByLevel.First());
            return true;
        }
        return false;
    }

    List<BasicAbility> CastableAbilities()
    {
        List<BasicAbility> validAbilities = new List<BasicAbility>();
        foreach (BasicAbility ability in Abilities)
            if (ability.CanCast())
                validAbilities.Add(ability);

        validAbilities = validAbilities.OrderBy(x => -x.levelRequired).ToList();
        if (validAbilities.Count > 1)
        {
            Debug.Log("sorted abilities");
        }
        return validAbilities;

    }


    void ChannelAbility() //only activated if cooldown is 0 and target isn't null
    {
        agent.SetDestination(MoveTarget.transform.position);

        BasicUnit initialAbilityTarget = MoveTargetUnit;
        if (currentAbility == null)
        {
            if (!PickAbility())
                return;
        }

        if (!currentAbility.IsValidTarget(initialAbilityTarget, false) || !currentAbility.CanCast())
        {
            if (debugMode)
                Debug.Log("Initial target no longer valid! Cancelling ability use.");
            InterruptAbility();
            return;
        }

        if (IsWithinRange(initialAbilityTarget.gameObject, currentAbility.range))
        {
            RotateTowards(initialAbilityTarget.gameObject.transform);
            //Debug.Log ("In range of ability!");
            if (currentAbility.CanCast() && !currentAbility.Running)
            {
                currentAbility.StartCasting(initialAbilityTarget);
                if (agent)
                    agent.Stop();
            }
        }


        if (currentAbility.Finished)
        {
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
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed / 20f);
    }

    void InterruptAbility()
    {
        SetAnimationState("Firing", false);
        if (currentAbility != null)
        { // && !currentAbility.finished
            if (debugMode)
                Debug.Log("Ability Interrupted");
            currentAbility.FinishAbility();
        }
        currentAbility = null;
        MoveTarget = null;

        if (agent)
            agent.Resume();
    }

    List<BasicUnit> ChooseAbilityAndFindPossibleTargets()
    {
        /*if (HasTag(Tag.Imperial) && HasTag(Tag.Hero))
        {
            string z = "";
        }*/
        foreach (BasicAbility potentialAbility in CastableAbilities())
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, huntSearchRadius);
            List<BasicUnit> acceptableTargets = new List<BasicUnit>();
            foreach (Collider collider in hitColliders)
            {
                BasicUnit PotentialTarget = collider.gameObject.GetComponent<BasicUnit>();
                if (potentialAbility.IsValidTarget(PotentialTarget, true))
                    acceptableTargets.Add(PotentialTarget);
            }
            if (acceptableTargets.Count > 0)
            {
                PickAbility(potentialAbility);
                return acceptableTargets;
            }
        }
        return new List<BasicUnit>();
    }

    public void TakeDamage(float damage, BasicUnit source, BasicBuff.Effect type)
    {
        timeSinceLastDamage = 0;

        //Apply defenses
        float reduction = 0;

        if (type == BasicBuff.Effect.KineticDamage)
            reduction = getResistKenetic;
        else if (type == BasicBuff.Effect.EnergyDamage)
            reduction = getResistEnergy;
        else if (type == BasicBuff.Effect.PsychicDamage)
            reduction = getResistPsychic;

        damage = Mathf.Max(damage * ((100f - reduction * 3) / 100f), 0);
        
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, getMaxHealth);

        if (currentHealth == 0 && Potions.Count > 0)
            UsePotion();

        if (passiveStates.Contains(currentState))
        {
            if (!Tags.Contains(Tag.Structure))
                StartHunting();
        }
        else if (combatStates.Contains(currentState))
        {
            if (Tags.Contains(Tag.Hero) && ShouldIFlee())
            {
                StopCombat();
                StartFleeing();
            }
            else if (!WithinActivationRange() && (currentAbility == null || currentAbility.Finished || !currentAbility.Running))
            {
                StopCombat();
                StartHunting();
            }
            else if (MoveTarget != null && (MoveTargetUnit.Tags.Contains(Tag.Structure) || Vector3.Distance(MoveTargetUnit.transform.position, transform.position) > maxHuntingDistance))
                StartHunting(source);
        }
        
        BroadcastCryForHelp(source);
    }

    public void TakeHealing(float damage, BasicUnit source)
    {
        currentHealth = Mathf.Clamp(currentHealth + damage, 0, getMaxHealth);
        if (currentHealth >= getMaxHealth && beingConstructed)
            FinishBuildingConstruction();

    }

    protected void GainEnergy(float energy)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + energy, 0, getMaxEnergy);
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

    protected bool IsWithinRange(GameObject potentialTarget, float range)
    {
        BasicUnit unit = potentialTarget.GetComponent<BasicUnit>();
        if(unit)
        {
            return (Vector3.Distance(transform.position, unit.transform.position) < range + GetRadius() + unit.GetRadius());
        }

        RaycastHit[] hitInfo = Physics.RaycastAll(transform.position, potentialTarget.transform.position - transform.position, range + GetRadius());
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
            //agent.speed = 0;
            agent.Stop();
            agent.enabled = false;
        }
        if (obstacle != null)
            obstacle.enabled = false;

        if (!Tags.Contains(Tag.Dead))
        { //then initialize the death state

            Tags.Add(Tag.Dead);
            remainingCorpseDuration = corpseDuration;

            if (Tags.Contains(Tag.Monster))
                OnDeathDistributeGoldAndXP();


            if (effectsProfile.DeathVFX != null)
            {
                GameObject deathEffect = (GameObject)Instantiate(effectsProfile.DeathVFX.gameObject, transform.position, transform.rotation);
                Destroy(deathEffect, effectsProfile.DeathVFX.duration);
            }

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
            if (NearbyUnit != null && !NearbyUnit.Tags.Contains(Tag.Structure) && !NearbyUnit.Tags.Contains(Tag.Dead) && NearbyUnit.team != team)
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
                goldRecipients[i].GainGold(goldGranted, true);
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
        return currentHealth >= getMaxHealth;
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
        int proposedNewLevel = (int)(XP/2);//Mathf.Sqrt(XP);
        while (Level < proposedNewLevel)
            LevelUp();
    }

    void LevelUp(bool healOnLevelUp = true) //add visual and sound effects
    {
        Level++;
        if (debugMode)
            Debug.Log(gameObject.name + " leveled up to " + Level + "!");
        //gameObject.name = (parentObjectName.Length ==0 ? gameObject.name : parentObjectName) + " " + Level;

        baseStats[Stat.Strength] += statBlock.levelUpStrength;
        baseStats[Stat.Dexterity] += statBlock.levelUpDexterity;
        baseStats[Stat.Intelligence] += statBlock.levelUpIntelligence;
        baseStats[Stat.Special] += statBlock.levelUpSpecial;
        baseStats[Stat.Sensitivity] += statBlock.levelUpSensitivity;

        if (healOnLevelUp)
            TakeHealing(getMaxHealth * levelUpHealAmount, this);

        if(HasTag(Tag.Hero))
        {
            GameObject levelUpVFX = (GameObject)Instantiate(AssetManager.Main.HeroLevelUpVFX.gameObject,transform.position,transform.rotation);
            levelUpVFX.transform.SetParent(transform);
            Destroy(levelUpVFX, 5f);
        }

    }
    
    //Crying for Help
    void BroadcastCryForHelp(BasicUnit attacker)
    {
        List<BasicUnit> nearbyUnits = GetUnitsWithinRange(cryForHelpRadius);
        foreach (BasicUnit unit in nearbyUnits)
        {
            if (unit.team == team)
                unit.HearCryForHelp(this, attacker);
        }

        foreach (GameObject unitObject in AllSpawns)
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
        return Level < LevelUnlocks.Count() - 1 && Level<AppManager.CurrentLevel.MaxCastleLevel;
    }

    public bool CanAffordToLevelUpStructure()
    {
        return TeamGold() >= LevelUpCost();
    }

    public int LevelUpCost()
    {
        return LevelUnlocks[Level + 1].cost;
    }

    public void LevelUpStucture()
    {
        team.Gold -= LevelUpCost();
        LevelUp(false);
        beingConstructed = true;
        GameManager.Main.PossibleOptionsChange(this);
    }

    //UI Considerations
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Main.StartInspection(this);
    }
    public void OnDrag(PointerEventData eventData)
    {
        UIScreenDrag.Main.OnDrag(eventData);
    }
    public void OnScroll(PointerEventData eventData)
    {
        UIScreenDrag.Main.OnScroll(eventData);
    }

    //Building Construction

    public bool beingConstructed = false;
    public void StartBuildingConstruction()
    {
        currentHealth = getMaxHealth * startingStructureHealthPercent;
        beingConstructed = true;
    }

    void FinishBuildingConstruction()
    {
        beingConstructed = false;
        GameManager.Main.PossibleOptionsChange(this);
    }

    //Entering Buildings and Housing Units


     Vector3 previousScale = new Vector3(1, 1, 1);
    //Vector3 previousPosition;
    void EnterStructure(BasicUnit structure)
    {
        structureOccupied = structure;
        structure.AdmitOccupant(this);

        Tags.Add(Tag.Inside); //Units that have the Inside tag are untargetable by abilities
        agent.enabled = false;

        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
            renderer.gameObject.SetActive(false);

        //previousPosition = structure.transform.position;

        previousScale = transform.localScale;

        transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
        //transform.position = structure.transform.position;
        //transform.position += new Vector3(0, -100, 0);
    }

    protected void LeaveStructure()
    {
        if (structureOccupied != null)
            structureOccupied.RemoveOccupant(this);
        structureOccupied = null;

        Tags.Remove(Tag.Inside);
        agent.enabled = true;

        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
            renderer.gameObject.SetActive(true);

        //transform.position = previousPosition;
        transform.localScale = previousScale;
    }

    protected void AdmitOccupant(BasicUnit newOccupant)
    {
        Occupants.Add(newOccupant);
    }

    protected bool RemoveOccupant(BasicUnit occupant)
    {
        return Occupants.Remove(occupant);
    }




    //Other Stuff
    bool UsePotion()
    {
        if (Potions.Count > 0)
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
        {
            team.structureCostMultiplier += structureCostReduction;
            team.researchTimeMultiplier += researchTimeReduction;
            team.researchCostMultiplier += researchCostReduction;
            team.RemoveUnit(this);
        }

        foreach (BasicUnit unit in Occupants)
        {
            if (unit != null)
                unit.LeaveStructure();
        }
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
        float scaledGoldCost = GoldCost + GoldCost * costScaling * GameManager.Main.Player.GetInstances(templateID).Count;
        if (HasTag(Tag.Structure) && HasTag(Tag.Imperial))
        {
            scaledGoldCost *= GameManager.Main.Player.structureCostMultiplier;
            scaledGoldCost = Mathf.RoundToInt(scaledGoldCost / 5.0f) * 5;
        }
        return (int)(scaledGoldCost);
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

    public void SwitchTeams(Team newTeam)
    {
        if (team != null)
            team.RemoveUnit(this);
        if (newTeam != null)
            newTeam.AddUnit(this);

        team = newTeam;
    }

    public float GetRadius()
    {
        if (agent)
            return agent.radius;
        else if (obstacle)
            return obstacle.radius;
        else
            return 0;
    }

    public float GetHeight()
    {
        if (agent)
            return agent.height;
        else if (obstacle)
            return obstacle.height;
        else
            return 0;
    }

}