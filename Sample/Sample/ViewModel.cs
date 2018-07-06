using System;
using Acr.XamForms;
using Plugin.HttpTransferTasks;


namespace Sample
{
    public abstract class ViewModel : AbstractNotifyPropertyChanged, IViewModelLifecycle
    {
        public virtual void OnActivated()
        {
        }


        public virtual void OnDeactivated()
        {
        }


        public virtual void OnOrientationChanged(bool isPortrait)
        {
        }


        public virtual bool OnBackRequested() => true;


        public virtual void OnDestroy()
        {
        }
    }
}
