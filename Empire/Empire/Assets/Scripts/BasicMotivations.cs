using UnityEngine;
using System.Collections.Generic;

public class BasicMotivations : MonoBehaviour {


    const float LoyaltyToBountyWeight = 2f;
    //the results of decision making

	[System.Serializable]
	public class Motive
	{
		public BasicUnit.State state;
		public float baseWeight;
        public List<Condition> conditionalModifiers;

        public float GetWeight(BasicUnit unit)
        {
            float finalWeight = baseWeight;
            foreach (Condition condition in conditionalModifiers)
            {
                finalWeight = condition.AdjustWeight(unit, finalWeight);
            }
            return finalWeight;
        }
	}

    [System.Serializable]
    public class Condition
    {
        public enum Type { HealthPercentage, PotionCount}
        public enum Comparison { LessThan, GreaterThan, Equals}
        public enum Operation { Add, Multiply}

        public Type testedValue;
        public Comparison comparison;
        public float threshold;
        public Operation modificationType;
        public float modificationValue;

        public float AdjustWeight(BasicUnit unit, float currentWeight)
        {
            
            float testedValue = float.NaN;
            bool conditionMet = false;

            switch (this.testedValue)
            {
                case Type.HealthPercentage:
                    testedValue = unit.getHealthPercentage;
                    break;
                case Type.PotionCount:
                    testedValue = unit.Potions.Count;
                    break;
                default:
                    break;
            }

            switch (comparison)
            {
                case Comparison.LessThan:
                    conditionMet = testedValue < threshold;
                    break;
                case Comparison.GreaterThan:
                    conditionMet = testedValue > threshold;
                    break;
                case Comparison.Equals:
                    conditionMet = testedValue == threshold;
                    break;
                default:
                    break;
            }

            if (conditionMet)
            {
                switch (modificationType)
                {
                    case Operation.Add:
                        currentWeight += modificationValue;
                        break;
                    case Operation.Multiply:
                        currentWeight *= modificationValue;
                        break;
                    default:
                        break;
                }
            }
                
            return currentWeight;
        }
    }

	public List<Motive> Motives;

    public BasicUnit.State CalculateState(BasicUnit unit)
    {
        Dictionary<BasicUnit.State, float> StateWeights = new Dictionary<BasicUnit.State, float>();
        
        foreach (Motive motive in Motives)
        {
            float weight = motive.GetWeight(unit);
            StateWeights.Add(motive.state, weight);
        }

        AdditionalModifiers(StateWeights, unit); //here's where we apply some custom logic

        float totalWeight = 0;
        foreach (float weight in StateWeights.Values)
            totalWeight += weight;

        float rolledWeight = Random.Range(0, totalWeight);

        foreach(BasicUnit.State state in StateWeights.Keys)
        {
            rolledWeight -= StateWeights[state];
            if (rolledWeight <= 0)
                return state;
        }

        throw new System.Exception("No state chosen!");
       // return BasicUnit.State.None;
    }

    void AdditionalModifiers(Dictionary<BasicUnit.State, float> StateWeights, BasicUnit unit)
    {
        if (StateWeights.ContainsKey(BasicUnit.State.KillBounty))
            StateWeights[BasicUnit.State.KillBounty] += unit.Loyalty * LoyaltyToBountyWeight;
        if (StateWeights.ContainsKey(BasicUnit.State.DefendBounty))
            StateWeights[BasicUnit.State.DefendBounty] += unit.Loyalty * LoyaltyToBountyWeight;
        if (StateWeights.ContainsKey(BasicUnit.State.ExploreBounty))
            StateWeights[BasicUnit.State.ExploreBounty] += unit.Loyalty * LoyaltyToBountyWeight;
    }
}
