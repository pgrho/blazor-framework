using System;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class WrapPanel<T> : ItemsControl<T>
        where T : class
    {
        protected override bool HasClientSize => _ClientWidth > 0 && _ClientHeight > 0;

        protected override bool SetScrollInfo(ScrollInfo info)
        {
            _ClientWidth = info.ClientWidth;
            _ClientHeight = info.ClientHeight;

            var c = Math.Max((int)Math.Floor(_ClientWidth / (ItemWidth + ItemMarginX)), 1);

            _Columns = c;

            var r = Math.Max((int)Math.Ceiling(_ClientHeight / (ItemHeight + ItemMarginY)), 1);

            Console.WriteLine("SetRange: {0} * {1}", c, r);

            return SetRange(0, c * r - 1);
        }

        int _Columns;

        float _ClientWidth;
        float _ClientHeight;
        float _ScrollLeft;
        float _ScrollTop;
        float _ScrollWidth;
        float _ScrollHeight;

        public override void OnScrolled(ItemsControlScrollInfo scrollInfo)
        {
            Console.WriteLine(
                "OnScrolled: {0} {1} {2} {3} {4}",
                scrollInfo.Viewport,
                scrollInfo.First,
                scrollInfo.Last,
                scrollInfo.MinWidth,
                scrollInfo.MinHeight);
        }
    }
}
