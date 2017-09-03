using System.ComponentModel;
using IContainer = Autofac.IContainer;

namespace AMaaS.Core.Sdk.Excel.UI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Fields

        private bool       _isBusy;
        private string     _busyMessage;
        private IContainer _container;

        #endregion

        #region Properties

        protected IContainer Container => AddinContext.Container;

        public virtual bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChange(nameof(IsBusy));
            }
        }

        public virtual string BusyMessage
        {
            get { return _busyMessage; }
            set
            {
                _busyMessage = value;
                RaisePropertyChange(nameof(BusyMessage));
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        protected virtual void RaisePropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
