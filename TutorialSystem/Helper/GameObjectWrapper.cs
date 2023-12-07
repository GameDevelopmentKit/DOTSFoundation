namespace TutorialSystem.Helper
{
    using UnityEngine;
    using UnityEngine.UI;

    public class GameObjectWrapper
    {
        public readonly GameObject    Value;
        private         Transform     OriginParent       { get; }
        private         int           OriginSiblingIndex { get; }
        private         RectTransform substituteTransform;

        public GameObjectWrapper(string name) : this(TutorialObjectCollection.GetObjectInstanceByPath(name)) { }

        public GameObjectWrapper(GameObject originObj)
        {
            this.Value = originObj;
            if (originObj == null) return;

            this.OriginSiblingIndex = this.Value.transform.GetSiblingIndex();
            this.OriginParent       = this.Value.transform.parent;
        }

        public void SetActive(bool isActive) { this.Value.SetActive(isActive); }

        public T GetComponent<T>() { return this.Value.GetComponent<T>(); }

        public void SetNewParent(Transform parent)
        {
            var originalLocalScale = this.Value.transform.localScale;
            this.Value.transform.SetParent(parent);
            this.Value.transform.SetAsLastSibling();
            if (this.OriginParent.GetComponent<LayoutGroup>())
            {
                var rectTransform = this.Value.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    this.substituteTransform = new GameObject($"{this.Value.name}_").AddComponent<RectTransform>();
                    this.substituteTransform.SetParent(this.OriginParent);
                    this.substituteTransform.localScale = originalLocalScale;
                    this.substituteTransform.sizeDelta  = rectTransform.sizeDelta;
                    this.substituteTransform.SetSiblingIndex(this.OriginSiblingIndex);
                }
            }
        }

        public void SetOriginParent()
        {
            if (this.substituteTransform != null)
                Object.Destroy(this.substituteTransform.gameObject);
            
            if(this.Value == null) return;
            this.Value.transform.SetParent(this.OriginParent);
            this.Value.transform.SetSiblingIndex(this.OriginSiblingIndex);
        }
    }
}