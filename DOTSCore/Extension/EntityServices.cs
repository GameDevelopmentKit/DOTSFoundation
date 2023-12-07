namespace DOTSCore.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Unity.Entities;

    public static class EntityServices
    {
        #region EntityManager

        public static void AddComponentData(this EntityManager em, Entity e, IComponentData c)
        {
            var cType = c.GetType();
            if (!AddComponentCache.TryGetValue(cType, out var action))
            {
                action                   = GenerateSetComponentDelegate(cType);
                AddComponentCache[cType] = action;
            }

            action(em, e, c);
        }

        private static readonly Dictionary<Type, Action<EntityManager, Entity, IComponentData>> AddComponentCache             = new();
        private static readonly MethodInfo                                                      AddComponentDataGenericMethod = FindAddComponentDataGenericMethod();

        private static MethodInfo FindAddComponentDataGenericMethod() =>
            typeof(EntityManager)
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(m =>
                {
                    if (m.Name != "AddComponentData" || !m.ContainsGenericParameters) return false;
                    var p = m.GetParameters();
                    return p.Length == 2 && p[0].ParameterType == typeof(Entity);
                }).First();

        private static Action<EntityManager, Entity, IComponentData> GenerateSetComponentDelegate(Type cType)
        {
            var methodInstance = AddComponentDataGenericMethod.MakeGenericMethod(cType);
            // return (manager, entity, componentData) => methodInstance.Invoke(manager, new object[] { entity, componentData });
            var              paramEm = Expression.Parameter(typeof(EntityManager), "em");
            var              paramE  = Expression.Parameter(typeof(Entity), "e");
            var              paramC  = Expression.Parameter(typeof(IComponentData), "c");
            Expression       castC   = Expression.Convert(paramC, cType);
            Expression       call    = Expression.Call(paramEm, methodInstance, paramE, castC);
            LambdaExpression lambda  = Expression.Lambda<Action<EntityManager, Entity, IComponentData>>(call, paramEm, paramE, paramC);
            return (Action<EntityManager, Entity, IComponentData>)lambda.Compile();
        }

        #endregion

        #region Entity Command Buffer

        public static void AddComponentData(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity e, IComponentData component)
        {
            var cType = component.GetType();
            if (!AddComponentEcbCache.TryGetValue(cType, out var action))
            {
                action                      = GenerateAddComponentEcbDelegate(cType);
                AddComponentEcbCache[cType] = action;
            }

            action(ecb, index, e, component);
        }

        private static readonly Dictionary<Type, Action<EntityCommandBuffer.ParallelWriter, int, Entity, IComponentData>> AddComponentEcbCache         = new();
        private static readonly MethodInfo                                                                                AddComponentEcbGenericMethod = FindAddComponentEcbGenericMethod();

        private static MethodInfo FindAddComponentEcbGenericMethod() =>
            typeof(EntityManager)
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Where(m =>
                {
                    if (m.Name != "AddComponent" || !m.ContainsGenericParameters) return false;
                    var p = m.GetParameters();
                    return p.Length == 3 && p[1].ParameterType == typeof(Entity);
                }).First();

        private static Action<EntityCommandBuffer.ParallelWriter, int, Entity, IComponentData> GenerateAddComponentEcbDelegate(Type cType)
        {
            var methodInstance = AddComponentEcbGenericMethod.MakeGenericMethod(cType);
            // return (manager, entity, componentData) => methodInstance.Invoke(manager, new object[] { entity, componentData });
            var              paramEcb = Expression.Parameter(typeof(EntityCommandBuffer.ParallelWriter), "ecb");
            var              paramI   = Expression.Parameter(typeof(int), "i");
            var              paramE   = Expression.Parameter(typeof(Entity), "e");
            var              paramC   = Expression.Parameter(typeof(IComponentData), "c");
            Expression       castC    = Expression.Convert(paramC, cType);
            Expression       call     = Expression.Call(paramEcb, methodInstance, paramI,paramE, castC);
            LambdaExpression lambda   = Expression.Lambda<Action<EntityCommandBuffer.ParallelWriter, int, Entity, IComponentData>>(call, paramEcb, paramI, paramE, paramC);
            return (Action<EntityCommandBuffer.ParallelWriter, int, Entity, IComponentData>)lambda.Compile();
        }

        #endregion
    }
}