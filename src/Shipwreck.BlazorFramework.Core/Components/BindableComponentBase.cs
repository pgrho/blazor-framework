using Microsoft.AspNetCore.Components;

namespace Shipwreck.BlazorFramework.Components
{
    public abstract partial class BindableComponentBase : ComponentBase
    {
    }

    public abstract partial class BindableComponentBase<T> : BindableComponentBase, IBindableComponent
        where T : class
    {
        object IBindableComponent.DataContext => DataContext;
    }
}
