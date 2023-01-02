namespace GASCore.UnityHybrid.HealthBar
{
    using UnityEngine;

    public class BillBoard : MonoBehaviour
    {
        private Transform  camTransform;
        private Quaternion originalRotation;
        public void Start()
        {
            var main = Camera.main;
            if (main == null) return;
            this.camTransform = main.transform;
            if (this.TryGetComponent<Canvas>(out var canvas))
            {
                canvas.worldCamera = main;
            }

            this.originalRotation = this.transform.localRotation;
        }


        public void Update() { this.transform.rotation = this.camTransform.rotation * this.originalRotation; }
    }
}