using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class WrapPanel<T> : ItemsControl<T>
        where T : class
    {
        #region ItemWidth

        private float _DefaultItemWidth = 100;
        private float? _MinItemWidth;

        [Parameter]
        public float DefaultItemWidth
        {
            get => _DefaultItemWidth;
            set => SetProperty(ref _DefaultItemWidth, value);
        }

        protected float ItemWidth => _MinItemWidth ?? DefaultItemWidth;

        #endregion ItemWidth

        #region ItemHeight

        private float _DefaultItemHeight = 100;
        private float? _MinItemHeight;

        [Parameter]
        public float DefaultItemHeight
        {
            get => _DefaultItemHeight;
            set => SetProperty(ref _DefaultItemHeight, value);
        }

        protected float ItemHeight => _MinItemHeight ?? DefaultItemHeight;

        #endregion ItemHeight



        protected override void UpdateRange(ScrollInfo info, int firstIndex, float localY)
        {
            var _ClientHeight = info.ClientHeight;
            var c = GetColumns(info);

            _Columns = c;

            var r = Math.Max((int)Math.Ceiling((_ClientHeight + localY) / ItemHeight), 1);

            Console.WriteLine($"ClientHeight={_ClientHeight}, ItemHeight={ItemHeight}, localY={localY}, rows={r}");

            SetVisibleRange(firstIndex, firstIndex + c * r - 1, localY);
        }

        private int GetColumns(ScrollInfo info)
        {
            var _ClientWidth = info.ClientWidth;

            var c = Math.Max((int)Math.Floor(_ClientWidth / ItemWidth), 1);
            return c;
        }

        protected override void SetScroll(ItemsControlScrollInfo info)
        {
            _MinItemWidth = info.MinWidth > 0 ? info?.MinWidth : null;
            _MinItemHeight = info.MinHeight > 0 ? info?.MinHeight : null;
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

            UpdateRange(info.Viewport, fi, ft);
        }

        protected override ValueTask ScrollAsync(int firstIndex, float localY)
        {
            return JS.scrollToItem(Element, ItemSelector, firstIndex, localY, _Columns, false);
            //return JS.ScrollTo(Element, 0, Math.Max(0, (ItemHeight + ItemMarginY) * (firstIndex / Math.Max(1, _Columns)) + localY), false);
        }

        private int _Columns;
    }
}
