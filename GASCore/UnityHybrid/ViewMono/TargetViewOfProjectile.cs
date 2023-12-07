namespace Gameplay.View.ViewMono
{
    using UnityEngine;

    public class TargetViewOfProjectile : MonoBehaviour
    {
        public GameObject targetView;
        public void UpdateTargetPosition(Vector3 position)
        {
            this.targetView.transform.position = position;
        }

    }
}