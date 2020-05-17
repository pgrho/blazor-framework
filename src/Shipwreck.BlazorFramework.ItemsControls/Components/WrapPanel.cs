using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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

        protected override void UpdateRange(ScrollInfo info, int firstIndex, float localY, bool forceScroll)
        {
            SetColumnCount(info);

            var r = Math.Max((int)Math.Ceiling((info.ClientHeight + localY) / ItemHeight), 1);

            SetVisibleRange(firstIndex, firstIndex + ColumnCount * r - 1, localY, forceScroll);
        }

        protected override void SetScroll(ItemsControlScrollInfo info, bool forceScroll)
        {
            SetColumnCount(info.Viewport);
            _MinItemWidth = info.MinWidth > 0 ? info?.MinWidth : null;
            _MinItemHeight = info.MinHeight > 0 ? info?.MinHeight : null;
            int fi;
            float ft;
            if (info.First != null)
            {
                ft = info.Viewport.ScrollTop - info.First.Top;

                var w = info.First.LastIndex + 1 - info.First.FirstIndex;
                if (w == 1)
                {
                    fi = info.First.FirstIndex;
                }
                else
                {
                    var rows = (w - 1) / ColumnCount + 1;
                    var rh = info.First.Height / rows;
                    var ri = Math.Min(Math.Max(0, (int)Math.Floor(ft / rh)), rows - 1);
                    fi = info.First.FirstIndex + ri * ColumnCount;
                    ft -= rh * ri;
                }
            }
            else
            {
                fi = 0;
                ft = 0;
            }

            UpdateRange(info.Viewport, fi, ft, forceScroll);
        }

        #region BuildRenderTree

        [Parameter]
        public string TagName { get; set; }

        protected override string GetTagName() => TagName ?? base.GetTagName();
         
        protected override void RenderFirstPadding(RenderTreeBuilder builder, ref int sequence)
            => RenderPaddingCore(
                builder,
                ref sequence,
                0,
                FirstIndex - 1,
                Math.Max(0, (FirstIndex + ColumnCount - 1) / ColumnCount) * ItemHeight);

        protected override void RenderLastPadding(RenderTreeBuilder builder, ref int sequence)
            => RenderPaddingCore(
                builder,
                ref sequence,
                LastIndex + 1,
                Source.Count - 1,
                Math.Max(0, (Source.Count - LastIndex + ColumnCount - 2) / ColumnCount * ItemHeight));

        #endregion BuildRenderTree
    }
}
