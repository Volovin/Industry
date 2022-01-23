using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Industry.UI.Windows.Buildings;

namespace Industry.UI.Windows
{
    public abstract class UIWindow : UIElement, IWindow
    {
        [SerializeField]
        private bool m_FadeBackground;

        [SerializeField]
        private UIWindowBackground background;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private string showTriggerName = "show";

        [SerializeField]
        private string closeTriggerName = "close";

        [SerializeField]
        private Text tittleText;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        protected RectTransform m_rectTransform;

        protected PopupController m_popupController;
        protected WindowsController m_windowsController;

        public bool FadeBackground
        {
            get
            {
                return m_FadeBackground;
            }
            set
            {
                m_FadeBackground = value;
            }
        }

        public bool IsShown
        {
            get; private set;
        }

        public string Tittle
        {
            get
            {
                return tittleText.text;
            }
            set
            {
                tittleText.text = value;
            }
        }

        public Rect BoundsRect
        {
            get { return m_rectTransform.rect; }
        }


        private void Awake()
        {
            OnAwakening();
        }

        private void Start()
        {
            m_windowsController = WindowsController.Instance;
            m_popupController = WindowsController.Instance.PopupController;

            if (backButton != null && !WindowsController.Instance.NavigationHistory)
                backButton.gameObject.SetActive(false);

            background.gameObject.SetActive(m_FadeBackground);
            SetWindowActive(false);

            OnStarting();
        }

        private IEnumerator DeactivateWindow()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            SetWindowActive(false);
        }

        protected void SetWindowActive(bool active)
        {
            gameObject.SetActive(active);

            if (m_FadeBackground)
                background.gameObject.SetActive(active);
        }

        public void Unhide()
        {
            if (IsShown)
                return;

            OnShowing();
            SetWindowActive(true);
            animator.SetTrigger(showTriggerName);

            if (m_FadeBackground)
                background.FadeIn();

            IsShown = true;
        }

        public void Hide()
        {
            if (!IsShown)
                return;

            if (!gameObject.activeInHierarchy)
                return;

            OnHiding();
            animator.SetTrigger(closeTriggerName);

            if (m_FadeBackground)
                background.FadeOut();

            IsShown = false;

            StartCoroutine(DeactivateWindow());
        }
        
        public void Show()
        {
            if (IsShown || m_windowsController.WindowsLocked)
                return;

            if (this is BuildingWindow && !m_windowsController.VerifyInputPosition())
                return;

            Unhide();

            WindowsController.Instance.RegisterWindow(this);
        }

        public void Close()
        {
            WindowsController.Instance.DeleteNavigationHistory();
            Hide();
        }

        public void Back()
        {
            WindowsController.Instance.ReturnToLastWindow();
        }

        protected abstract void OnAwakening();
        protected abstract void OnStarting();
        protected abstract void OnShowing();
        protected abstract void OnHiding();
    }
}