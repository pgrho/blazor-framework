using Microsoft.AspNetCore.Components;

namespace Shipwreck.BlazorFramework.Components
{
    public abstract partial class BindableLayoutComponentBase : LayoutComponentBase
    {
    }

    public abstract partial class BindableLayoutComponentBase<T> : BindableLayoutComponentBase, IBindableComponent
        where T : class
    {
        object IBindableComponent.DataContext => DataContext;
    }
}
