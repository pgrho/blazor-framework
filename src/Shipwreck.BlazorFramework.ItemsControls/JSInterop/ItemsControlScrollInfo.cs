using System.Collections.Generic;

namespace Shipwreck.BlazorFramework.JSInterop
{
    public sealed class ItemsControlScrollInfo
    {
        public ScrollInfo Viewport { get; set; }
        public ItemsControllElementInfo First { get; set; }
        public ItemsControllElementInfo Last { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }

        public IList<ItemsControllLineInfo> Lines { get; set; }
    }
}
