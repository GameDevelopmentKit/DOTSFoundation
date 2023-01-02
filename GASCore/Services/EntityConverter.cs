namespace GASCore.Services
{
    using System;
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using Newtonsoft.Json;
    using UnityEngine;

    public static class EntityConverter
    {
        [Serializable]
        public class EntityData<T> where T : IComponentConverter
        {
            [SerializeReference] public List<T> components = new();
        }

        public static string ConvertEntitiesDataToJson<T>(this List<EntityData<T>> listEntityData) where T : IComponentConverter
        {
            return JsonConvert.SerializeObject(listEntityData, EntitySerializerSetting.Value);
        }

        public static List<EntityData<T>> ConvertJsonToEntitiesData<T>(this string json) where T : IComponentConverter
        {
            if (string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<List<EntityData<T>>>(json, EntitySerializerSetting.Value);
        }
        
        public static string ConvertComponentsDataToJson<T>(this List<T> listEntityData) where T : IComponentConverter
        {
            return JsonConvert.SerializeObject(listEntityData, EntitySerializerSetting.Value);
        }

        public static List<T> ConvertJsonToComponentsData<T>(this string json) where T : IComponentConverter
        {
            if (string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<List<T>>(json, EntitySerializerSetting.Value);
        }
    }
}