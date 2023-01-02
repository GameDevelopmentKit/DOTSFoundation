namespace GASCore.UnityHybrid.HealthBar
{
    using UnityEngine;

    public class SeparateLine : MonoBehaviour
    {
        [SerializeField] private int   divideNumber = 1;
        
        private RectTransform rectTransform;
        
        public int DivideNumber => this.divideNumber;

        private void Awake()
        {
            if (this.rectTransform == null)
            {
                this.rectTransform = this.GetComponent<RectTransform>();
            }
        }

        public void SetupSeparateLine(float posX)
        {
            this.rectTransform.anchoredPosition = new Vector2(posX, 0 );
        }
    }
}