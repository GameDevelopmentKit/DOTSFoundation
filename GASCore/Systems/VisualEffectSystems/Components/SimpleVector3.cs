namespace GASCore.Systems.VisualEffectSystems.Components
{
    using System;
    using Unity.Mathematics;
    using UnityEngine;

    [Serializable]
    public struct SimpleVector3
    {
        public float X;
        public float Y;
        public float Z;
        
        
        /// <summary>
        /// Converts a SimpleVector3 to float3.
        /// </summary>
        /// <param name="v">SimpleVector3 to convert.</param>
        /// <returns>The converted float3.</returns>
        public static implicit operator float3(SimpleVector3 v) { return new float3(v.X, v.Y, v.Z); }
    }
}