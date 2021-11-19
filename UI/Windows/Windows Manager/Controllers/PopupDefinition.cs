using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Industry.UI.Windows
{
    [Serializable]
    public class PopupDefinition
    {
        [SerializeField]
        private string tittleText;

        [SerializeField]
        private string descriptionText;

        [SerializeField]
        private string confirmText;

        [SerializeField]
        private bool hasCancelButton = true;

        [SerializeField]
        private string cancelText;

        public string TittleText
        {
            get { return tittleText; }
        }

        public string DescriptionText
        {
            get { return descriptionText; }
        }

        public string ConfirmText
        {
            get { return confirmText; }
        }

        public bool HasCancelButton
        {
            get { return hasCancelButton; }
        }

        public string CancelText
        {
            get { return cancelText; }
        }
    }
}