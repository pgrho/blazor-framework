using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class ListScope<T> : ListComponentBase<T>
        where T : class
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
            => builder.AddContent(0, ChildContent);
    }
}
