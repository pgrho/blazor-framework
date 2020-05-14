using System;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class StackPanel<T> : ItemsControl<T>
        where T : class
    {
        protected override bool HasClientSize => _ClientHeight > 0;

        protected override bool SetScrollInfo(ScrollInfo info)
        {
            _ClientHeight = info.ClientHeight;

            var r = Math.Max((int)Math.Ceiling(_ClientHeight / (ItemHeight + ItemMarginY)), 1);

            return SetRange(0, r - 1);
        }

        float _ClientHeight;
        float _ScrollTop;
        float _ScrollHeight;
         
        public override void OnScrolled(ItemsControlScrollInfo scrollInfo)
        {
            Console.WriteLine(
                "OnScrolled: {0} {1} {2} {3}",
                scrollInfo.Viewport,
                scrollInfo.First,
                scrollInfo.Last,
                scrollInfo.MinHeight);
        }
    }
}
