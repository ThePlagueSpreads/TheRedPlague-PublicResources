using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Utilities;

public static class CustomExosuitArmUtils
{
    private static readonly List<CustomArm> CustomArmData = new();

    public static void RegisterCustomExosuitArm(CustomArm arm)
    {
        CustomArmData.Add(arm);
    }

    public readonly struct CustomArm
    {
        public CustomArm(TechType techType, Func<IOut<GameObject>, IEnumerator> prefab)
        {
            TechType = techType;
            Prefab = prefab;
        }

        public TechType TechType { get; }
        public Func<IOut<GameObject>, IEnumerator> Prefab { get; }
    }

    public static IReadOnlyList<CustomArm> GetCustomExosuitArms() => CustomArmData;
}