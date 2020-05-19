using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class ScrollableTable<T> : StackPanel<T>
        where T : class
    {
        public ScrollableTable()
        {
            ItemSelector = ":scope > table > tbody > tr[data-itemindex]";
        }

        [CascadingParameter]
        public ScrollableTableTheme Theme { get; set; }

        [Parameter]
        public string HeaderHeight { get; set; }

        #region Wrapper

        [Parameter]
        public string ElementClass { get; set; }

        protected virtual IEnumerable<KeyValuePair<string, object>> GetElementAttributes()
        {
            var attrs = AdditionalAttributes.MergeAttributes(Theme?.ElementAttributes).AppendClass(ElementClass ?? Theme?.ElementClass);

            var h = HeaderHeight ?? Theme?.HeaderHeight;
            if (h != null)
            {
                attrs = attrs.PrependStyle("padding-top:" + h);
            }
            return attrs;
        }

        #endregion Wrapper

        #region Header Background

        [Parameter]
        public string HeaderBackgroundClass { get; set; }

        [Parameter]
        public string HeaderBackgroundBackground { get; set; }

        [Parameter]
        public Dictionary<string, object> HeaderBackgroundAttributes { get; set; }

        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }

        protected virtual IEnumerable<KeyValuePair<string, object>> GetHeaderBackgroundAttributes()
        {
            var attrs = HeaderBackgroundAttributes.MergeAttributes(Theme?.HeaderBackgroundAttributes)
                            .AppendClass(HeaderBackgroundClass ?? Theme?.HeaderBackgroundClass);

            var s = "position: absolute;top: 0;width: 100%;";
            var h = HeaderHeight ?? Theme?.HeaderHeight;
            if (h != null)
            {
                s += ";height:" + h;
            }
            var bg = HeaderBackgroundBackground ?? Theme?.HeaderBackgroundBackground;
            if (bg != null)
            {
                s += ";background:" + bg;
            }
            return attrs.PrependStyle(s);
        }

        #endregion Header Background

        #region Scroller

        [Parameter]
        public string ScrollerClass { get; set; }

        [Parameter]
        public Dictionary<string, object> ScrollerAttributes { get; set; }

        protected virtual IEnumerable<KeyValuePair<string, object>> GetScrollerAttributes()
            => ScrollerAttributes.MergeAttributes(Theme?.ScrollerAttributes)
                    .AppendClass(ScrollerClass ?? Theme?.ScrollerClass)
                    .PrependStyle("overflow-x: hidden;overflow-y: auto;min-height: 80%;max-height: 100%;-webkit-overflow-scrolling: touch;");

        #endregion Scroller

        #region Table

        [Parameter]
        public string TableClass { get; set; }

        [Parameter]
        public Dictionary<string, object> TableAttributes { get; set; }

        protected virtual IEnumerable<KeyValuePair<string, object>> GetTableAttributes()
            => TableAttributes.MergeAttributes(Theme?.TableAttributes)
                    .AppendClass(TableClass ?? Theme?.TableClass)
                    .PrependStyle("overflow-anchor: none");

        #endregion Table

        #region Table Head

        [Parameter]
        public string TableHeadClass { get; set; }

        [Parameter]
        public Dictionary<string, object> TableHeadAttributes { get; set; }

        protected virtual IEnumerable<KeyValuePair<string, object>> GetTableHeadAttributes()
            => TableHeadAttributes.MergeAttributes(Theme?.TableHeadAttributes)
                .AppendClass(TableHeadClass ?? Theme?.TableHeadClass);

        #endregion Table Head

        #region Body

        [Parameter]
        public string TableBodyClass { get; set; }

        [Parameter]
        public Dictionary<string, object> TableBodyAttributes { get; set; }

        protected virtual IEnumerable<KeyValuePair<string, object>> GetTableBodyAttributes()
            => TableBodyAttributes.MergeAttributes(Theme?.TableBodyAttributes)
                .AppendClass(TableBodyClass ?? Theme?.TableBodyClass)
                .PrependStyle("overflow-anchor:none");

        #endregion Body

        protected override void RenderFirstPadding(RenderTreeBuilder builder, ref int sequence, int firstIndex)
            => RenderPaddingCore(
                builder,
                ref sequence,
                0,
                firstIndex - 1,
                Math.Max(0, (firstIndex + ColumnCount - 1) / ColumnCount) * ItemHeight, tagName: "tr");

        protected override void RenderLastPadding(RenderTreeBuilder builder, ref int sequence, int lastIndex)
            => RenderPaddingCore(
                builder,
                ref sequence,
                lastIndex + 1,
                Source.Count - 1,
                Math.Max(0, (Source.Count - lastIndex + ColumnCount - 2) / ColumnCount * ItemHeight), tagName: "tr");
    }
}
