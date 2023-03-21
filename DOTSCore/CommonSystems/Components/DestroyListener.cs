namespace DOTSCore.CommonSystems.Components
{
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using UnityEngine;

    public class DestroyListener : MonoBehaviour
    {
        private int deadTrigger = Animator.StringToHash(Components.AnimationTrigger.Dead.ToString());
        
        public virtual void DestroyGameObject(GameObjectHybridLink gameObjectHybridLink)
        {
            gameObjectHybridLink.Animator.SetTrigger(this.deadTrigger);
        }
        public void OnDeadAnimFinish()
        {
            this.Recycle();
        }
    }
}