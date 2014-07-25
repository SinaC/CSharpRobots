﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSharpRobotsWPF.MVVM
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            MemberExpression body = propertyExpression.Body as MemberExpression;

            if (body == null)
                throw new ArgumentException(@"Invalid argument", "propertyExpression");

            PropertyInfo property = body.Member as PropertyInfo;

            if (property == null)
                throw new ArgumentException(@"Argument is not a property", "propertyExpression");

            return property.Name;
        }

        protected bool Set<T>(Expression<Func<T>> selectorExpression, ref T field, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;
            field = newValue;
            string propertyName = GetPropertyName(selectorExpression);
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool Set<T>(string propertyName, ref T field, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
