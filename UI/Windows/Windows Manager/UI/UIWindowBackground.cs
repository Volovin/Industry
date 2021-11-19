using UnityEngine;
using System.Collections;

namespace Industry.UI.Windows
{
    public class UIWindowBackground : UIElement
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private string fadeInTriggerName = "fadeIn";

        [SerializeField]
        private string fadeOutTriggerName = "fadeOut";

        public void FadeIn()
        {
            animator.SetTrigger(fadeInTriggerName);
        }

        public void FadeOut()
        {
            animator.SetTrigger(fadeOutTriggerName);
        }
    }
}