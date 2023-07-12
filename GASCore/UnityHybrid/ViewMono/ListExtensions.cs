namespace GASCore.UnityHybrid.ViewMono
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Element<T>
    {
        public float value;
        public T     item;

        public void Deconstruct(out float value, out T item)
        {
            value = this.value;
            item  = this.item;
        }
    }

    public static class ListExtensions
    {
        public static void Init<T>(this List<Element<T>> list)
        {
            list.Sort((a, b) => b.value.CompareTo(a.value));
        }

        public static T Get<T>(this List<Element<T>> list, float currentValue)
        {
            foreach (var (value, item) in list)
            {
                if (currentValue >= value) return item;
            }

            return default;
        }
    }
}