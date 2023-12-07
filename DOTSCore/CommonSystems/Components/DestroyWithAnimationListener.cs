namespace DOTSCore.CommonSystems.Components
{
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using UnityEngine;

    public interface IDestroyListener
    {
        public void DestroyGameObject();
    }
    
    [RequireComponent(typeof(Animator))]
    public class DestroyWithAnimationListener : MonoBehaviour,IDestroyListener
    {
        public           bool              isDestroy   = true;
        private          int               deadTrigger = Animator.StringToHash(AnimationTrigger.Dead.ToString());
        
        public virtual void DestroyGameObject()
        {
            this.GetComponent<Animator>().SetTrigger(this.deadTrigger);
            this.gameObject.SetActive(false);
        }
        public void OnDeadAnimFinish()
        {
            if (this.isDestroy)
            {
                this.Recycle();
            }
        }
    }
}