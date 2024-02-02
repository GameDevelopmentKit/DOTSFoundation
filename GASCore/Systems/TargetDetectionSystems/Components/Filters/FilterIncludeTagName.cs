namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Entities;

    public struct FilterIncludeTagName : IBufferElementData
    {
        public                          FixedString64Bytes Value;
        public static implicit operator FixedString64Bytes(FilterIncludeTagName tagName) => tagName.Value;
        public static implicit operator FilterIncludeTagName(string tagName)             => new() { Value = tagName };

        public class Option : IFilterTargetConverter
        {
            public List<string> TagNames;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                var tagsToInclude = ecb.AddBuffer<FilterIncludeTagName>(index, entity);
                foreach (var tagNames in this.TagNames)
                {
                    tagsToInclude.Add((FilterIncludeTagName)tagNames);
                }
            }
        }
    }
}