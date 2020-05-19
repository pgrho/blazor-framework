using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class ListScope<T> : ListComponentBase<T>
        where T : class
    {
        [Parameter]
        public RenderFragment<T> ItemTemplate { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Source != null)
            {
                foreach (var e in Source)
                {
                    builder.AddContent(0, ItemTemplate(e));
                }
            }
        }
    }
}
