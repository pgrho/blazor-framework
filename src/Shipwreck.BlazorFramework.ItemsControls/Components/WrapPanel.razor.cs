using System;
using System.Threading.Tasks;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class WrapPanel<T> : ItemsControl<T>
        where T : class
    {
        protected override bool HasClientSize => _ClientWidth > 0 && _ClientHeight > 0;

        protected override bool SetRange(ScrollInfo info, int firstIndex)
        {
            var c = GetColumns(info);

            _Columns = c;

            var r = Math.Max((int)Math.Ceiling(_ClientHeight / (ItemHeight + ItemMarginY)), 1);

            return SetRange(firstIndex, firstIndex + c * r - 1);
        }

        private int GetColumns(ScrollInfo info)
        {
            _ClientWidth = info.ClientWidth;
            _ClientHeight = info.ClientHeight;

            var c = Math.Max((int)Math.Floor(_ClientWidth / (ItemWidth + ItemMarginX)), 1);
            return c;
        }

        protected override bool SetScroll(ItemsControlScrollInfo info)
        {
            int fi;
            float ft;
            if (info.First != null)
            {
                ft = info.Viewport.ScrollTop - info.First.Top;

                _Columns = GetColumns(info.Viewport);

                var c = Math.Max(1, (info.First.LastIndex + 1 - info.First.FirstIndex) / _Columns);
                if (c == 1)
                {
                    fi = info.First.FirstIndex;
                }
                else
                {
                    var li = (int)Math.Floor(ft * c / info.First.Height);
                    fi = info.First.FirstIndex + li;
                    ft -= info.First.Height * li / c;
                }
            }
            else
            {
                fi = 0;
                ft = 0;
            }

            _ScrollTopInFirstItem = ft;

            return SetRange(info.Viewport, fi);
        }

        protected override ValueTask ScrollAsync()
        {
            StateHasChanged();

            return JS.ScrollTo(Element, 0, Math.Max(0, (ItemHeight + ItemMarginY) * (FirstIndex / Math.Max(1, _Columns)) + _ScrollTopInFirstItem), false);
        }

        private float _ScrollTopInFirstItem;

        private int _Columns;

        private float _ClientWidth;
        private float _ClientHeight;
    }
}
