namespace TutorialSystem.Components
{
    using System;
    using System.Collections.Generic;
    using TaskModule.Authoring;
    using TutorialSystem.Helper;
    using Unity.Entities;
    using UnityEngine;

    public class HighlightUIArea : IComponentData, ITaskActiveActionComponentConverter
    {
        public string           GameObjectPath;
        public bool             IsForce;
        public List<EffectInfo> Effects;

        internal GameObjectWrapper ForcedObject;
        [Serializable]
        public class EffectInfo
        {
            public string EffectAddressablePath;
            public bool   IsFistSiblingInTransform;
            
            public AnchorPreset EffectAnchor = AnchorPreset.MiddleCenter;
            public Vector2      Offset;

            internal GameObject LoadedEffectObject;
        }

        public void Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponentData(taskEntity, this); }
    }
}