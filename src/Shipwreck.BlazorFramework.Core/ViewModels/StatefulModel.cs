using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shipwreck.BlazorFramework.ViewModels
{
    public abstract partial class StatefulModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
