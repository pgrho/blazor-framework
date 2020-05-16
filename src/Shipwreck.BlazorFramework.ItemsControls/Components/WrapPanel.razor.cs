using System;
using Microsoft.AspNetCore.Components;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class WrapPanel<T> : ItemsControl<T>
        where T : class
    {
        protected override int ColumnCount => _Columns;
        private int _Columns = 1;

        private int SetColumnCount(ScrollInfo info)
            => _Columns = Math.Max((int)Math.Floor(info.ClientWidth / ItemWidth), 1);

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
            SetColumnCount(info);

            var r = Math.Max((int)Math.Ceiling((info.ClientHeight + localY) / ItemHeight), 1);

            SetVisibleRange(firstIndex, firstIndex + ColumnCount * r - 1, localY);
        }

        protected override void SetScroll(ItemsControlScrollInfo info)
        {
            SetColumnCount(info.Viewport);
            _MinItemWidth = info.MinWidth > 0 ? info?.MinWidth : null;
            _MinItemHeight = info.MinHeight > 0 ? info?.MinHeight : null;
            int fi;
            float ft;
            if (info.First != null)
            {
                ft = info.Viewport.ScrollTop - info.First.Top;

                var c = Math.Max(1, (info.First.LastIndex + 1 - info.First.FirstIndex) / ColumnCount);
                if (c == 1)
                {
                    fi = info.First.FirstIndex;
                }
                else
                {
                    var li = Math.Max(0, (int)Math.Floor(ft * c / info.First.Height));
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
    }
}
