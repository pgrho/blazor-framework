using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Shipwreck.BlazorFramework.ViewModels;

namespace Shipwreck.BlazorFramework.Components
{
    partial class BindableComponentBase
    {
        protected bool SetProperty(ref string field, string value, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            if (value != field)
            {
                field = value;
                onChanged?.Invoke();
                if (propertyName != null) RaisePropertyChanged(propertyName, render);
                return true;
            }
            return false;
        }

        protected bool SetProperty<TValue>(ref TValue field, TValue value, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            if (!((field as IEquatable<TValue>)?.Equals(value) ?? Equals(field, value)))
            {
                field = value;
                onChanged?.Invoke();
                if (propertyName != null) RaisePropertyChanged(propertyName, render);
                return true;
            }
            return false;
        }

        protected bool SetFlagProperty(ref byte field, byte flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (byte)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }

        protected bool SetFlagProperty(ref ushort field, ushort flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (ushort)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }

        protected bool SetFlagProperty(ref uint field, uint flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (uint)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }

        protected bool SetFlagProperty(ref ulong field, ulong flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (ulong)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }
        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null, bool render = true)
        {
            if (render) StateHasChanged();
        }
    }
    partial class BindableLayoutComponentBase
    {
        protected bool SetProperty(ref string field, string value, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            if (value != field)
            {
                field = value;
                onChanged?.Invoke();
                if (propertyName != null) RaisePropertyChanged(propertyName, render);
                return true;
            }
            return false;
        }

        protected bool SetProperty<TValue>(ref TValue field, TValue value, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            if (!((field as IEquatable<TValue>)?.Equals(value) ?? Equals(field, value)))
            {
                field = value;
                onChanged?.Invoke();
                if (propertyName != null) RaisePropertyChanged(propertyName, render);
                return true;
            }
            return false;
        }

        protected bool SetFlagProperty(ref byte field, byte flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (byte)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }

        protected bool SetFlagProperty(ref ushort field, ushort flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (ushort)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }

        protected bool SetFlagProperty(ref uint field, uint flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (uint)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }

        protected bool SetFlagProperty(ref ulong field, ulong flag, bool hasFlag, Action onChanged = null, [CallerMemberName]string propertyName = null, bool render = true)
        {
            var nv = (ulong)(hasFlag ? (field | flag) : (field & ~flag));
            return SetProperty(ref field, nv, onChanged, propertyName, render: render);
        }
        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null, bool render = true)
        {
            if (render) StateHasChanged();
        }
    }
}
namespace Shipwreck.BlazorFramework.Components
{
    partial class BindableComponentBase<T>
    {
        private T _DataContext;

        [Parameter]
        public virtual T DataContext
        {
            get => _DataContext;
            set
            {
                var prev = _DataContext;
                if (value != prev)
                {
                    OnDataContextRemoved(prev);
                    _DataContext = value;
                    OnDataContextSet(value);
                    StateHasChanged();
                }
            }
        }

        protected virtual void OnDataContextRemoved(T dataContext)
        {
            if (dataContext is INotifyPropertyChanged n)
            {
                n.PropertyChanged -= DataContext_PropertyChanged;
            }
            if (dataContext is IRequestFocus r)
            {
                r.FocusRequested -= DataContext_FocusRequested;
            }
        }

        protected virtual void OnDataContextSet(T dataContext)
        {
            if (dataContext is INotifyPropertyChanged n)
            {
                n.PropertyChanged += DataContext_PropertyChanged;
            }
            if (dataContext is IRequestFocus r)
            {
                r.FocusRequested += DataContext_FocusRequested;
            }
        }
        
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (OnDataContextPropertyChanged(e.PropertyName))
            {
                StateHasChanged();
            }
        }
        
        protected virtual bool OnDataContextPropertyChanged(string propertyName)
            => true;

        private void DataContext_FocusRequested(string obj)
            => OnDataContextRequestedFocus(obj);

        protected virtual bool OnDataContextRequestedFocus(string propertyName)
        {
            Console.WriteLine("Failed to focus element for property {0}", propertyName);
            return false;
        }
    }
    partial class BindableLayoutComponentBase<T>
    {
        private T _DataContext;

        [Parameter]
        public virtual T DataContext
        {
            get => _DataContext;
            set
            {
                var prev = _DataContext;
                if (value != prev)
                {
                    OnDataContextRemoved(prev);
                    _DataContext = value;
                    OnDataContextSet(value);
                    StateHasChanged();
                }
            }
        }

        protected virtual void OnDataContextRemoved(T dataContext)
        {
            if (dataContext is INotifyPropertyChanged n)
            {
                n.PropertyChanged -= DataContext_PropertyChanged;
            }
            if (dataContext is IRequestFocus r)
            {
                r.FocusRequested -= DataContext_FocusRequested;
            }
        }

        protected virtual void OnDataContextSet(T dataContext)
        {
            if (dataContext is INotifyPropertyChanged n)
            {
                n.PropertyChanged += DataContext_PropertyChanged;
            }
            if (dataContext is IRequestFocus r)
            {
                r.FocusRequested += DataContext_FocusRequested;
            }
        }
        
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (OnDataContextPropertyChanged(e.PropertyName))
            {
                StateHasChanged();
            }
        }
        
        protected virtual bool OnDataContextPropertyChanged(string propertyName)
            => true;

        private void DataContext_FocusRequested(string obj)
            => OnDataContextRequestedFocus(obj);

        protected virtual bool OnDataContextRequestedFocus(string propertyName)
        {
            Console.WriteLine("Failed to focus element for property {0}", propertyName);
            return false;
        }
    }
    partial class ListComponentBase<TDataContext, TItem>
    {
        private TDataContext _DataContext;

        [Parameter]
        public virtual TDataContext DataContext
        {
            get => _DataContext;
            set
            {
                var prev = _DataContext;
                if (value != prev)
                {
                    OnDataContextRemoved(prev);
                    _DataContext = value;
                    OnDataContextSet(value);
                    StateHasChanged();
                }
            }
        }

        protected virtual void OnDataContextRemoved(TDataContext dataContext)
        {
            if (dataContext is INotifyPropertyChanged n)
            {
                n.PropertyChanged -= DataContext_PropertyChanged;
            }
            if (dataContext is IRequestFocus r)
            {
                r.FocusRequested -= DataContext_FocusRequested;
            }
        }

        protected virtual void OnDataContextSet(TDataContext dataContext)
        {
            if (dataContext is INotifyPropertyChanged n)
            {
                n.PropertyChanged += DataContext_PropertyChanged;
            }
            if (dataContext is IRequestFocus r)
            {
                r.FocusRequested += DataContext_FocusRequested;
            }
        }
        
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (OnDataContextPropertyChanged(e.PropertyName))
            {
                StateHasChanged();
            }
        }
        
        protected virtual bool OnDataContextPropertyChanged(string propertyName)
            => true;

        private void DataContext_FocusRequested(string obj)
            => OnDataContextRequestedFocus(obj);

        protected virtual bool OnDataContextRequestedFocus(string propertyName)
        {
            Console.WriteLine("Failed to focus element for property {0}", propertyName);
            return false;
        }
    }
}