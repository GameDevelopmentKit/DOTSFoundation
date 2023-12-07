namespace TutorialSystem.View
{
    using TMPro;
    using UnityEngine;

    public class TutorialDialogueUI : MonoBehaviour
    {
        public TextMeshProUGUI Content;
        
        public void SetContent(string content) { Content.text = content; }
    }
}