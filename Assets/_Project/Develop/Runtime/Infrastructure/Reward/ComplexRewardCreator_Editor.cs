#if UNITY_EDITOR

using Infrastructure.JsonCreator;
using UnityEngine;

namespace Infrastructure.Reward
{
    [CreateAssetMenu(fileName = "ComplexRewardCreator_Editor", menuName = "Config/JsonCreator/ComplexRewardCreator")]
    public class ComplexRewardCreator_Editor : JsonCreator_Editor<ComplexReward>
    {
    
    }
}

#endif