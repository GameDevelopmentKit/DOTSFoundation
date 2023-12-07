namespace GASCore.Systems.VisualEffectSystems.Components
{
    using System;
    using Sirenix.OdinInspector;
    using Unity.Mathematics;
    using UnityEngine;

    [Serializable]
    public struct SimpleVector3
    {
        [HorizontalGroup(), LabelWidth(30)]
        public float x;
        [HorizontalGroup(), LabelWidth(30)]
        public float y;
        [HorizontalGroup(), LabelWidth(30)]
        public float z;


        /// <summary>
        /// Converts a SimpleVector3 to float3.
        /// </summary>
        /// <param name="v">SimpleVector3 to convert.</param>
        /// <returns>The converted float3.</returns>
        public static implicit operator float3(SimpleVector3 v) { return new float3(v.x, v.y, v.z); }
    }

    [Serializable]
    public struct SimpleFloatVector2
    {
        [HorizontalGroup(), LabelWidth(30)]
        public float x;
        [HorizontalGroup(), LabelWidth(30)]
        public float y;

        public static implicit operator float2(SimpleFloatVector2 v) { return new float2(v.x, v.y); }
    }

    [Serializable]
    public struct SimpleIntVector2
    {
        [HorizontalGroup(), LabelWidth(30)]
        public int x;
        [HorizontalGroup(), LabelWidth(30)]
        public int y;

        public static implicit operator int2(SimpleIntVector2 v) { return new int2(v.x, v.y); }
    }
    
    [Serializable]
    public struct SimpleFloatRange
    {
        [HorizontalGroup(), LabelWidth(30)]
        public float min;
        [HorizontalGroup(), LabelWidth(30)]
        public float max;

        public static implicit operator float2(SimpleFloatRange v) { return new float2(v.min, v.max); }
    }

    [Serializable]
    public struct SimpleIntRange
    {
        [HorizontalGroup(), LabelWidth(30)]
        public int min;
        [HorizontalGroup(), LabelWidth(30)]
        public int max;

        public static implicit operator int2(SimpleIntRange v) { return new int2(v.min, v.max); }
    }

    [Serializable]
    public struct SimpleBool3
    {
        [HorizontalGroup, LabelWidth(30)] public bool x;
        [HorizontalGroup, LabelWidth(30)] public bool y;
        [HorizontalGroup, LabelWidth(30)] public bool z;

        public SimpleBool3(bool x, bool y, bool z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator bool3(SimpleBool3 v) => new(v.x, v.y, v.z);
    }
}