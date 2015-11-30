using UnityEngine;
using System.Collections;

public class BasicProjectile : MonoBehaviour {
    BasicUnit Target;
    float Speed = 10f;
    BasicAbility Ability;
    

	// Use this for initialization
	void Start () {
        Destroy(gameObject, GetComponent<ParticleSystem>().duration);
	}

    public void Initialize(BasicUnit Target, float Speed, BasicAbility Ability)
    {
        this.Target = Target;
        this.Speed = Speed;
        this.Ability = Ability;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Playing)
        {
            if (Target != null)
            {
                Vector3 targetLocation = Target.transform.position + new Vector3(0, 1, 0);

                transform.position = Vector3.MoveTowards(transform.position, targetLocation, Speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetLocation) <= 0)
                {
                    if (Ability != null)
                    {
                        Ability.ProjectileLanded(Target);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
