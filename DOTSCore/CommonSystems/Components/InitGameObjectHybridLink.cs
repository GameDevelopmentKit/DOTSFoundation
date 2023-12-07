namespace DOTSCore.CommonSystems.Components
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    public class InitGameObjectHybridLink : MonoBehaviour
    {
        private IEntityViewMono[]   listEntityViewMono;
        private IViewMonoListener[] listViewMonoListener;
        private Animator            animator;

        private IEntityViewMono[] ListEntityViewMono
        {
            get
            {
                if (this.listEntityViewMono == null)
                {
                    listEntityViewMono = this.GetComponentsInChildren<IEntityViewMono>();
                }

                return this.listEntityViewMono;
            }
        }

        private IViewMonoListener[] ListViewMonoListener
        {
            get
            {
                if (this.listViewMonoListener == null)
                {
                    listViewMonoListener = this.GetComponentsInChildren<IViewMonoListener>();
                }

                return this.listViewMonoListener;
            }
        }

        private Animator Animator
        {
            get
            {
                if (this.animator == null)
                {
                    animator = this.GetComponent<Animator>();
                }

                return this.animator;
            }
        }

        public void Init(EntityManager entityManager, Entity entity)
        {
            foreach (var viewMono in ListEntityViewMono)
            {
                viewMono.BindEntity(entityManager, entity);
            }

            if (ListViewMonoListener.Length > 0)
            {
                var listenerCollector = entityManager.TryGetListenerCollector(entity);
                foreach (var viewMonoListener in listViewMonoListener)
                {
                    viewMonoListener.RegisterEvent(listenerCollector);
                }
            }

            entityManager.AddComponentData(entity, new GameObjectHybridLink
            {
                Value = this.gameObject
            });

            if (this.Animator != null)
            {
                entityManager.AddComponentData(entity, new AnimatorHybridLink()
                {
                    Value = Animator
                });
            }
        }
    }
}