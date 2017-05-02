using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;


namespace Plugin.HttpTransferTasks
{
    public abstract class AbstractNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SuppressChangeNotifications { get; set; }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!this.SuppressChangeNotifications)
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Member not found for expression");

            this.OnPropertyChanged(member.Member.Name);
        }


        protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
                return false;

            property = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
