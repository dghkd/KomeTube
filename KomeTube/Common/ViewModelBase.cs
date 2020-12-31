using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace KomeTube.Common
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Private Properties

        private readonly Dictionary<String, PropertyChangedEventArgs> _cacheEventArgs;

        #endregion Private Properties

        #region Constructor

        protected ViewModelBase()
        {
            _cacheEventArgs = new Dictionary<String, PropertyChangedEventArgs>();
        }

        #endregion Constructor

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            PropertyChangedEventArgs args;
            if (!_cacheEventArgs.TryGetValue(propertyName, out args))
            {
                args = new PropertyChangedEventArgs(propertyName);
                _cacheEventArgs.Add(propertyName, args);
            }

            OnPropertyChanged(args);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }

        #endregion INotifyPropertyChanged Members
    }
}