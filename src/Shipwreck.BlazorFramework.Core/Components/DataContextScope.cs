using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Shipwreck.BlazorFramework.Components
{
    public class DataContextScope : BindableComponentBase<object>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
            => builder.AddContent(0, ChildContent);

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public IEnumerable<string> DependsOnProperties { get; set; }

        [Parameter]
        public IEnumerable<string> IgnoresProperties { get; set; }

        protected override bool OnDataContextPropertyChanged(string propertyName)
            => DependsOnProperties?.Contains(propertyName) != false
            && IgnoresProperties?.Contains(propertyName) != true;
    }
}
