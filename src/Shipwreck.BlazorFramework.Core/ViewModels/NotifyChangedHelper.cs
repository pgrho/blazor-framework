using System.Collections.Specialized;
using System.ComponentModel;

namespace Shipwreck.BlazorFramework.ViewModels
{
    public static class NotifyChangedHelper
    {
        public static void AddPropertyChanged(this INotifyPropertyChanged obj, PropertyChangedEventHandler handler)
        {
            if (obj != null)
            {
                obj.PropertyChanged += handler;
            }
        }

        public static void RemovePropertyChanged(this INotifyPropertyChanged obj, PropertyChangedEventHandler handler)
        {
            if (obj != null)
            {
                obj.PropertyChanged -= handler;
            }
        }

        public static void AddCollectionChanged(this INotifyCollectionChanged obj, NotifyCollectionChangedEventHandler handler)
        {
            if (obj != null)
            {
                obj.CollectionChanged += handler;
            }
        }

        public static void RemoveCollectionChanged(this INotifyCollectionChanged obj, NotifyCollectionChangedEventHandler handler)
        {
            if (obj != null)
            {
                obj.CollectionChanged -= handler;
            }
        }
    }
}
