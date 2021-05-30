using System;
using System.IO;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    public class DocumentControl : ContentControl
    {
        private bool _hasUnsavedChanges = false;
        private bool _firstTimeLoad = true;
        private DateTime _lastLoadTime;


        internal event Action<String> NameChanged = delegate { };
        internal event Action Activated = delegate { };


        protected sealed override void OnLoad()
        {
            base.OnLoad();

            if (_firstTimeLoad)
            {
                _lastLoadTime = DateTime.Now;
                OnFirstLoad();
                _firstTimeLoad = false;
            }

            //if this function is called again, it will be probably
            //because it has been activated
            //if we are closing, don't trigger the activated event
            else if (!IsClosing)
            {
                Activate();
            }
        }


        protected virtual void OnFirstLoad()
        {
        }




        internal void FileContentUpdate()
        {
            AlertFileSave();
            OnFirstLoad();
        }


        protected virtual void OnActivate()
        {
        }


        protected virtual void OnClose(bool shouldSave)
        {
        }


        public void Close(bool shouldSave)
        {
            OnClose(shouldSave);
        }


        /// <summary>
        /// Alert the containing tab that the content has been changed.
        /// </summary>
        public void AlertFileChange()
        {
            _hasUnsavedChanges = true;
            ReviewFormName();
        }


        public void AlertFileSave()
        {
            _lastLoadTime = DateTime.Now;
            _hasUnsavedChanges = false;
            ReviewFormName();
        }


        internal void ReviewFormName()
        {
            NameChanged.Invoke(_hasUnsavedChanges ? FileItem.FileName + "*" : FileItem.FileName);
        }

        
        


        public FileItem FileItem
        {
            get;
            internal set;
        }


        public bool HasUnsavedChanges
        {
            get { return _hasUnsavedChanges; }
        }


        public bool IsClosing
        {
            get;
            internal set;
        }


        internal DateTime LastLoadTime
        {
            get { return _lastLoadTime; }
            set { _lastLoadTime = value; }
        }

        public DocumentAreaWindow DocumentAreaWindow
        {
            get { return this.GetAncestors().OfType<DocumentAreaWindow>().First(); }
        }


        /*
        public UIControl Content
        {
            get;
            set;
        }*/
        public void Activate()
        {
            OnActivate();
            Activated.Invoke();
        }
    }
}