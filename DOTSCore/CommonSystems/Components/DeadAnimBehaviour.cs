using DOTSCore.CommonSystems.Components;
using UnityEngine;

public class DeadAnimBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"DeadAnimBehaviour - OnStateExit");
        animator.transform.GetComponent<DestroyListener>().OnDeadAnimFinish();
    }
}
