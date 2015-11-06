using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
//using System;

public class BasicUnit : MonoBehaviour,  IPointerClickHandler{
    public string parentObjectName;
    public enum Tag { Structure, Organic, Imperial, Monster, Mechanical, Dead, Store }
    public enum State { None, Deciding, Exploring, Hunting, InCombat, Following, Shopping, GoingHome, Fleeing, Relaxing, Sleeping, Dead, Structure } //the probabilities of which state results after "Deciding" is determined per class
    public enum Attribute { MoveSpeed, AttackSpeed, AttackDamage, MaxHealth, HealthRegen, MaxMana, ManaRegen }

    public Team team;

    public int Gold;
    public float XP; //make private
    public int Level;

    [System.Serializable]
    public class EquipmentSlot
    {
        public BasicItem.ItemType Type;
        public BasicItem Instance;
    }
    public EquipmentSlot[] EquipmentSlots;
    public BasicItem[] ProductsSold; 

    public int GoldCost;

    //public Tag[] InitialTags;//only for initial tags in the inspector
    public List<Tag> Tags;

    public GameObject MoveTarget;
    BasicUnit MoveTargetUnit { get { return MoveTarget ? MoveTarget.GetComponent<BasicUnit>() : null; } }
    public Vector3 ExploreTarget;

    GameObject Home;
    NavMeshAgent agent;
    LineRenderer lineRenderer;
    new Renderer renderer;

    //state logic
    public State currentState;

    //Combat Traits
    public float maxHealth;
    float currentHealth;
    float huntSearchRadius = 20;
    float storeSearchRadius = 1000;
    float deathGiftRadius = 20f;
    public float attackRadius = 4;
    public float attackDamage;
    public float attackCooldown;
    float remainingAttackCooldown;
    float attackDuration = 1f;
    float remainingAttackDuration;
    
    public List<Tag> AbilityTags;
    public List<Tag> RequiredAbilityTags;
    public List<Tag> ExcludedAbilityTags;

    //Combat Accessors
    float FinalAttackDamage { get { return attackDamage + Level; } }

    //Movement Traits
    float MoveSpeed;

    //Automatic Spawning
    //public bool Structure = false;
    public bool abilityHeals = false;
    public bool autoSpawningEnabled = false;
    public int spawnGoldCost;
    public GameObject SpawnType;
    public int MaxSpawns = 0;
    public float SpawnCooldown = 3;
    private float RemainingSpawnCooldown;
    private List<GameObject> Spawns;

    float corpseDuration = 0;
    float remainingCorpseDuration;

    bool spawnedByStructure = false;
    bool canLevelUp = false;
    


    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();
        renderer = GetComponent<Renderer>();
        currentHealth = maxHealth;
        Spawns = new List<GameObject>();

        Tags = Tags ?? new List<Tag>();
        AbilityTags = AbilityTags ?? new List<Tag>();
        RequiredAbilityTags = RequiredAbilityTags ?? new List<Tag>();
        ExcludedAbilityTags = ExcludedAbilityTags ?? new List<Tag>();

        XP = Mathf.Pow(Level,2);

        if (team == null && Tags.Contains(Tag.Imperial)) // && !spawnedByStructure
            team = GameManager.Main.Player;


        if (Tags.Contains(Tag.Structure)) //Structure Setup
        {
            //currentState = State.None; //just for now, will modify thinking table when implemented
            corpseDuration = 0;
            currentState = State.Structure;
        }
        else //Normal Unit Setup
        {
            currentState = State.Deciding;
            corpseDuration = 10;
            MoveSpeed = agent.speed;
            agent.stoppingDistance = attackRadius - 1;
            if (Tags.Contains(Tag.Imperial))
                canLevelUp = true;
        }


        remainingAttackDuration = attackDuration;
        remainingCorpseDuration = corpseDuration;
        
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

        UpdateSpawning();
        CleanUp();
	}

    void ExploreLogic()
    {
        if (ExploreTarget == Vector3.zero)
            ExploreTarget = new Vector3(Random.Range(0,GameManager.Main.MapBounds.x), 2, Random.Range(0,GameManager.Main.MapBounds.z));
        agent.SetDestination(ExploreTarget);
        if (Vector3.Distance(ExploreTarget, transform.position) < 5)
            StopExploring();
    }

    void StopExploring()
    {
        currentState = State.Deciding;
        ExploreTarget = Vector3.zero;
    }

    void DecideLogic()
    {
        MoveTarget = null;
        ExploreTarget = Vector3.zero;

        float RandomSelection = Random.Range(0, 100);
        if (RandomSelection < 50)
            currentState = State.Hunting;
        else if (RandomSelection < 60)
            currentState = State.Exploring;
        else
            currentState = State.Shopping;
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
            if (Vector3.Distance(MoveTarget.transform.position, transform.position) < agent.stoppingDistance + 1)
                BrowseWares();
        }
    }

    void BrowseWares()
    {
        int amount = new List<BasicItem>(MoveTargetUnit.ProductsSold).Count; //if you put this in the for loop it fucking explodes

        // try {
        for (int product = 0; product < amount; product++)
        {
            for (int slot = 0; slot < EquipmentSlots.Count(); slot++)
            {
                BasicItem soldItem = MoveTargetUnit.ProductsSold[product];
                if (EquipmentSlots[slot].Type == soldItem.Type)
                {
                    if (EquipmentSlots[slot].Instance == null || soldItem.Level > EquipmentSlots[slot].Instance.Level)
                    {
                        if (soldItem.Cost < Gold)
                        {
                            Gold -= soldItem.Cost;
                            MoveTargetUnit.GainGold(soldItem.Cost);
                            EquipmentSlots[slot].Instance = Instantiate(soldItem.gameObject).GetComponent<BasicItem>();
                            Debug.Log(gameObject.name + " bought " + EquipmentSlots[slot].Instance.name + " from " + MoveTarget + ".");
                            DoneShopping();
                        }
                    }
                }
            }
        }
        DoneShopping();
        /*  }
        catch
         {
             Debug.Log("What fuckery is this");
         }*/
    }

    void DoneShopping()
    {
        MoveTarget = null;
        currentState = State.Deciding;
    }

    bool FindStore()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, storeSearchRadius);
        List<BasicUnit> acceptableTargets = new List<BasicUnit>();
        foreach (Collider collider in hitColliders)
        {
            BasicUnit encounteredUnit = collider.gameObject.GetComponent<BasicUnit>();
            if (encounteredUnit != null && encounteredUnit.Tags.Contains(Tag.Store) && encounteredUnit.team == team)
                acceptableTargets.Add(encounteredUnit);
        }

        if (acceptableTargets.Count > 0)
        {
            MoveTarget = acceptableTargets[Random.Range(0, acceptableTargets.Count - 1)].gameObject;
            Debug.Log(gameObject.name + " chooses to shop at " + MoveTarget.name);
            return true;
        }
        

        return false;
    }

    void CleanUp()
    {
        if (currentHealth <= 0)
            Die();

        while (Spawns.Contains(null))
            Spawns.Remove(null);

        remainingAttackCooldown -= Time.deltaTime;
        remainingAttackCooldown = Mathf.Max(0, remainingAttackCooldown);

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
            Spawn();
        }
    }

    public bool CanSpawn()
    {
        return spawnGoldCost <= Gold;
    }

    public void Spawn()
    {
        RemainingSpawnCooldown = SpawnCooldown;

        GameObject spawnedObject = (GameObject)Instantiate(SpawnType, transform.position, transform.rotation);
        Spawns.Add(spawnedObject);
        BasicUnit spawnedUnit = spawnedObject.GetComponent<BasicUnit>();
        spawnedUnit.Home = gameObject;
        spawnedUnit.spawnedByStructure = true;
        spawnedUnit.parentObjectName = SpawnType.gameObject.name;

            //spawnedByStructure

        //spawn.GetComponent<BasicUnit>().team = team; //right now it is automatically being set by monster or imperial tag
    }

    void HuntingLogic()
    {
        
        if(MoveTarget == null) //find unit to focus on
        {
            lineRenderer.enabled = false;
            List<BasicUnit> acceptableTargets = AcquireAcceptableTargets();

            acceptableTargets = SortByDistance(acceptableTargets);

            if (acceptableTargets.Count != 0)
                MoveTarget = acceptableTargets[0].gameObject; //Random.Range(0, acceptableTargets.Count) //pick closest target
        }

        if (MoveTarget != null)
        {
            //Move Towards Target's new position
            agent.SetDestination(MoveTarget.transform.position);

            //Ability Logic
            ChannelAbility();
        }
        else
            StopHunting();
    }

    void StopHunting()
    {
        MoveTarget = null;
        currentState = State.Deciding;
    }

    List<BasicUnit> SortByDistance(List<BasicUnit> PotentialTargets)
    {
        return PotentialTargets.OrderBy(o => Vector3.Distance(o.transform.position,transform.position)).ToList();
    }


    void ChannelAbility() //only activated if cooldown is 0
    {
        BasicUnit AttackTarget = MoveTargetUnit;
        if (AttackTarget.Tags.Contains(Tag.Dead))
            EndAbility();
        else if (Vector3.Distance(transform.position, MoveTarget.transform.position) < attackRadius)
        {
            if (remainingAttackCooldown <= 0)
            {
                if (remainingAttackDuration > 0)
                {
                    remainingAttackDuration -= Time.deltaTime;
                    
                    lineRenderer.enabled = true;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, MoveTarget.transform.position);
                    if (abilityHeals)
                        AttackTarget.TakeHealing(FinalAttackDamage * Time.deltaTime);
                    else
                        AttackTarget.TakeDamage(FinalAttackDamage * Time.deltaTime);
                }
                if (remainingAttackDuration <= 0)
                    EndAbility();
            }
        }
        else
            remainingAttackDuration = attackDuration;
    }

    void EndAbility()
    {
        MoveTarget = null;
        lineRenderer.enabled = false;
        remainingAttackDuration = attackDuration;
        remainingAttackCooldown = attackCooldown;

    }



    List<BasicUnit> AcquireAcceptableTargets()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, huntSearchRadius);
        List<BasicUnit> acceptableTargets = new List<BasicUnit>();
        foreach (Collider collider in hitColliders)
        {
            BasicUnit PotentialTarget = collider.gameObject.GetComponent<BasicUnit>();
            if (PotentialTarget != null && PotentialTarget != this)
            {
                bool acceptableTarget = false;

                foreach (Tag tag in AbilityTags)
                    if (PotentialTarget.Tags.Contains(tag))
                        acceptableTarget = true;

                foreach (Tag tag in RequiredAbilityTags)
                    if (!PotentialTarget.Tags.Contains(tag))
                        acceptableTarget = false;

                foreach (Tag tag in ExcludedAbilityTags)
                    if (PotentialTarget.Tags.Contains(tag))
                        acceptableTarget = false;

                if (abilityHeals && PotentialTarget.AtMaxHealth())
                    acceptableTarget = false;

                if (acceptableTarget)
                    acceptableTargets.Add(PotentialTarget);
            }
        }
        return acceptableTargets;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= Mathf.Max(0,damage);
        if (currentState != State.Hunting)
            DecideLogic();


    }

    public void TakeHealing(float damage)
    {
        currentHealth += Mathf.Max(0,damage);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    void Die()
    {
        if (currentState != State.Dead) //then initialize the death state
        {
            currentState = State.Dead;
            Tags.Add(Tag.Dead);

            if (Tags.Contains(Tag.Monster))
                OnDeathDistributeGoldAndXP();

            if (agent != null)
                MoveTarget = null;

            ExploreTarget = Vector3.zero;

            renderer.material.color = Color.grey;
        }

        remainingCorpseDuration -= Time.deltaTime;
        if (remainingCorpseDuration <= 0)
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
            if (NearbyUnit != null && !NearbyUnit.Tags.Contains(Tag.Structure) && !NearbyUnit.Tags.Contains(Tag.Dead) && NearbyUnit.team != team && NearbyUnit.canLevelUp)
                xpRecipients.Add(col.gameObject.GetComponent<BasicUnit>());
        }
        foreach (BasicUnit xpRecipient in xpRecipients)
            xpRecipient.GainXP(Level / (float)goldRecipients.Count);
    }

    public bool AtMaxHealth()
    {
        return currentHealth >= maxHealth;
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
        Debug.Log(gameObject.name + " earned " + xpAmount + " XP");

        XP += xpAmount; 
        int proposedNewLevel = (int)Mathf.Sqrt(XP);
        while (Level < proposedNewLevel && canLevelUp)
            LevelUp();
    }

    void LevelUp()
    {
       
        Level++; //add visual and sound effects
        Debug.Log(gameObject.name + " leveled up to " + Level + "!");
        gameObject.name = (parentObjectName.Length ==0 ? gameObject.name : parentObjectName) + " " + Level;
    }


    //UI Considerations
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Main.StartInspection(this);
    }
}
