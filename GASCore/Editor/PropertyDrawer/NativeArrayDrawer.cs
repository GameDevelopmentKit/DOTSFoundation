namespace GASCore.Editor.PropertyDrawer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Unity.Collections;
    using UnityEngine;

    public class NativeListResolver<TElement> : BaseCollectionResolver<NativeList<TElement>> where TElement : unmanaged, IEquatable<TElement>
    {
        private Dictionary<int, InspectorPropertyInfo>           childInfos     = new();
        private int                                              lastUpdateId   = -1;
        private Dictionary<NativeList<TElement>, List<TElement>> elementsArrays = new();

        private HashSet<NativeList<TElement>> seenHashset  = new();
        private List<NativeList<TElement>>    toRemoveList = new();

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            if (childIndex < 0 || childIndex >= this.ChildCount)
                throw new IndexOutOfRangeException();
            InspectorPropertyInfo childInfo;
            if (!this.childInfos.TryGetValue(childIndex, out childInfo))
            {
                childInfo = InspectorPropertyInfo.CreateValue(CollectionResolverUtilities.DefaultIndexToChildName(childIndex), childIndex, this.Property.BaseValueEntry.SerializationBackend,
                    new GetterSetter<NativeList<TElement>, TElement>(
                        (ref NativeList<TElement> collection) => this.GetElement(collection, childIndex),
                        (ref NativeList<TElement> collection, TElement element) => this.SetElement(collection, element, childIndex)),
                    this.Property.Attributes.Where(attr => !attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true))
                        .AppendWith(new DelayedAttribute()).AppendWith(new SuppressInvalidAttributeErrorAttribute()).ToArray());
                this.childInfos[childIndex] = childInfo;
            }

            return childInfo;
        }

        private TElement GetElement(NativeList<TElement> collection, int index)
        {
            this.EnsureUpdated();
            List<TElement> elementList;
            return this.elementsArrays.TryGetValue(collection, out elementList) ? elementList[index] : default(TElement);
        }

        private void SetElement(NativeList<TElement> collection, TElement element, int index)
        {
            int            count = collection.Length;
            List<TElement> range;
            if (!this.elementsArrays.TryGetValue(collection, out range) || range.Contains(element))
                return;
            range[index] = element;
            collection.Clear();
            foreach (var element1 in range)
                collection.Add(element1);
            this.EnsureUpdated(true);
        }

        private void EnsureUpdated(bool force = false)
        {
            int updateId = this.Property.Tree.UpdateID;
            if (!force && this.lastUpdateId == updateId)
                return;
            this.seenHashset.Clear();
            this.toRemoveList.Clear();
            this.lastUpdateId = updateId;
            int valueCount = this.ValueEntry.ValueCount;
            for (int index = 0; index < valueCount; ++index)
            {
                NativeList<TElement> collection = this.ValueEntry.Values[index];
                if ((object)collection != null)
                {
                    this.seenHashset.Add(collection);
                    List<TElement> elementList;
                    if (!this.elementsArrays.TryGetValue(collection, out elementList))
                    {
                        elementList                     = new List<TElement>(collection.Length);
                        this.elementsArrays[collection] = elementList;
                    }

                    elementList.Clear();
                    elementList.AddRange((IEnumerable<TElement>)collection);
                    DictionaryKeyUtility.KeyComparer<TElement> keyComparer = DictionaryKeyUtility.KeyComparer<TElement>.Default;
                    elementList.Sort((IComparer<TElement>)keyComparer);
                }
            }

            foreach (NativeList<TElement> key in this.elementsArrays.Keys)
            {
                if (!this.seenHashset.Contains(key))
                    this.toRemoveList.Add(key);
            }

            for (int index = 0; index < this.toRemoveList.Count; ++index)
                this.elementsArrays.Remove(this.toRemoveList[index]);
        }

        public override    int  ChildNameToIndex(string name)                                       => CollectionResolverUtilities.DefaultChildNameToIndex(name);
        protected override int  GetChildCount(NativeList<TElement> value)                           => value.IsCreated ? value.Length : 0;
        public override    bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info) => false;
        protected override void Add(NativeList<TElement> collection, object value)                  => collection.Add((TElement)value);
        protected override void Clear(NativeList<TElement> collection)                              => collection.Clear();
        protected override bool CollectionIsReadOnly(NativeList<TElement> collection)               => false;
        protected override void Remove(NativeList<TElement> collection, object value)               => collection.RemoveAt(collection.IndexOf((TElement)value));
        public override    Type ElementType                                                         => typeof(TElement);

        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}