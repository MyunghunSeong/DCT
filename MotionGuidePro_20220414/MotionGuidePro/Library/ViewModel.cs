using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CrevisLibrary
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public Object m_Wnd;

        public ViewModel(Object Wnd)
        {
            m_Wnd = Wnd;
        }

        public ViewModel() {}

        public abstract void InitializeViewModel();

        #region 프로퍼티 이벤트
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}
