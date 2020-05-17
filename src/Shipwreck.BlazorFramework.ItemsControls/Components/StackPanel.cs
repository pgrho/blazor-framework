using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class StackPanel<T> : ItemsControl<T>
        where T : class
    {
        protected override int ColumnCount => 1;

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

        protected override void SetScroll(ItemsControlScrollInfo info, bool forceScroll)
        {
            _MinItemHeight = info.MinHeight > 0 ? info?.MinHeight : null;
            int fi;
            float ft;
            if (info.First != null)
            {
                ft = info.Viewport.ScrollTop - info.First.Top;

                var c = Math.Max(1, info.First.LastIndex + 1 - info.First.FirstIndex);
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

            UpdateRange(info.Viewport, fi, ft, forceScroll);
        }

        protected override void UpdateRange(ScrollInfo info, int firstIndex, float localY, bool forceScroll)
        {
            var r = Math.Max((int)Math.Ceiling((info.ClientHeight + localY) / ItemHeight), 1);

            SetVisibleRange(firstIndex, firstIndex + r - 1, localY, forceScroll);
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
                ScrollingFirstIndex - 1,
                Math.Max(
                    0,
                    ScrollingFirstIndex * ItemHeight));

        protected override void RenderLastPadding(RenderTreeBuilder builder, ref int sequence)
            => RenderPaddingCore(
                builder,
                ref sequence,
                LastIndex + 1,
                Source.Count - 1,
                Math.Max(0, (Source.Count - LastIndex - 1) * ItemHeight));

        #endregion BuildRenderTree
    }
}
