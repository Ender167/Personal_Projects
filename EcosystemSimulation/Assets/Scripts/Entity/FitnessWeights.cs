using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FitnessWeights
{
    public float maxHungerWeight = 0;
    public float hungerGrowthRateWeight = 0;
    public float hungerGrowthFrequencyWeight = 0;
    public float hungerDangerThresholdWeight = 0;
    public float hungerThresholdForFoodSearchWeight = 0;
    public float maxReproductionWeight = 0;
    public float reproductionThresholdForMateSearchWeight = 0;
    public float reproductionGrowthRateWeight = 0;
    public float reproductionGrowthFrequencyWeight = 0;
    public float movementSpeedWeight = 0;
    public float maxLookupRangeWeight = 0;
    public float wanderIntervalWeight = 0;
    public float maxHealthPointsWeight = 0;
}
