namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Entities;

    public struct FilterExcludeTagName : IBufferElementData
    {
        public                          FixedString64Bytes Value;
        public static implicit operator FixedString64Bytes(FilterExcludeTagName tagName) => tagName.Value;
        public static implicit operator FilterExcludeTagName(string tagName)             => new() { Value = tagName };

        public class Option : IFilterTargetConverter
        {
            public List<string> TagNames;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                var tagsToExclude = ecb.AddBuffer<FilterExcludeTagName>(index, entity);
                foreach (var tagName in this.TagNames)
                {
                    tagsToExclude.Add((FilterExcludeTagName)tagName);
                }
            }
        }
    }
}