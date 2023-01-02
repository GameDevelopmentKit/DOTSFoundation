namespace GASCore.UnityHybrid.HealthBar
{
    using System.Collections.Generic;
    using System.Linq;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIHpBarView : MonoBehaviour
    {
        [SerializeField] private Image              hpBar, shieldBar;
        [SerializeField] private RectTransform      separateGroup;
        [SerializeField] private List<SeparateLine> divideNumberToSeparateLine;

        [SerializeField] private float stepValue = 100;
        [SerializeField] private float hpValue;
        [SerializeField] private float shieldValue;
        [SerializeField] private float maxHpValue;

        public Image ImgHpBar     => this.hpBar;
        public Image ImgShieldBar => this.shieldBar;

        private void OnValidate() { this.divideNumberToSeparateLine = this.divideNumberToSeparateLine.OrderByDescending(line => line.DivideNumber).ToList(); }

        public int GetCurrentHealthPoint() => (int)this.hpValue;

        public void Init(float hpVal, float maxHeal, float shieldVal)
        {
            this.hpValue     = hpVal;
            this.maxHpValue  = maxHeal;
            this.shieldValue = shieldVal;
            this.SetupHealthBar();
        }

        public void UpdateHealBar(float healVal, float shieldVal)
        {
            this.hpValue     = healVal;
            this.shieldValue = shieldVal;
            this.SetupHealthBar();
        }
        
        [Button]
        private void SetupHealthBar()
        {
            // // set hp fill + shield fill 
            var maxBarValue = (this.shieldValue + this.hpValue) < this.maxHpValue ? this.maxHpValue : this.shieldValue + this.hpValue;

            this.hpBar.fillAmount     = this.hpValue / maxBarValue;
            this.shieldBar.fillAmount = (this.shieldValue + this.hpValue) / maxBarValue;

            // set separate line

            var childObj = this.separateGroup.GetComponentsInChildren<Image>();
            foreach (var child in childObj) child.Recycle();

            var amountLine       = Mathf.CeilToInt(maxBarValue / this.stepValue) - 1;
            var spaceBetweenLine = this.stepValue / maxBarValue * this.separateGroup.rect.width;

            for (var i = 1; i <= amountLine; i++)
            {
                foreach (var separateLine in this.divideNumberToSeparateLine)
                {
                    if (i % separateLine.DivideNumber == 0)
                    {
                        separateLine.Spawn(this.separateGroup).SetupSeparateLine(i * spaceBetweenLine);
                        break;
                    }
                }
            }
        }
    }
}