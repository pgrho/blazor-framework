using System;
using System.Collections.Generic;
using System.Linq;
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

        [Parameter]
        public string HeaderHeight { get; set; }

        #region Wrapper

        [Parameter]
        public string WrapperClass { get; set; }


        private IEnumerable<KeyValuePair<string, object>> GetWrapperAttributes()
        {
            var wrapperAttrs = AdditionalAttributes ?? Enumerable.Empty<KeyValuePair<string, object>>();
            if (WrapperClass != null)
            {
                wrapperAttrs = AddClass(wrapperAttrs, WrapperClass);
            }
            if (HeaderHeight != null)
            {
                wrapperAttrs = AddStyle(wrapperAttrs, "padding-top:" + HeaderHeight);
            }
            return wrapperAttrs;
        }


        #endregion

        #region Header

        [Parameter]
        public string HeaderBackgroundClass { get; set; }

        [Parameter]
        public string HeaderBackground { get; set; }

        [Parameter]
        public Dictionary<string, object> HeaderAttributes { get; set; }

        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }

        private IEnumerable<KeyValuePair<string, object>> GetHeaderAttributes()
        {
            var headerAttrs = HeaderAttributes ?? Enumerable.Empty<KeyValuePair<string, object>>();
            if (HeaderBackgroundClass != null)
            {
                headerAttrs = AddClass(headerAttrs, HeaderBackgroundClass);
            }
            var headerStyle = "position: absolute;top: 0;width: 100%;";

            if (HeaderHeight != null)
            {
                headerStyle += ";height:" + HeaderHeight;
            }
            if (HeaderBackground != null)
            {
                headerStyle += ";background:" + HeaderBackground;
            }
            return AddStyle(headerAttrs, headerStyle);
        }

        #endregion

        #region Scroller
        [Parameter]
        public string ScrollerClass { get; set; }

        [Parameter]
        public Dictionary<string, object> ScrollerAttributes { get; set; }


        private IEnumerable<KeyValuePair<string, object>> GetScrollerAttributes()
        {
            var scrollerAttrs = ScrollerAttributes ?? Enumerable.Empty<KeyValuePair<string, object>>();
            if (ScrollerClass != null)
            {
                scrollerAttrs = AddClass(scrollerAttrs, ScrollerClass);
            }

            return AddStyle(scrollerAttrs, "overflow-x: hidden;overflow-y: auto;min-height: 80%;max-height: 100%;-webkit-overflow-scrolling: touch;");
        }
        #endregion

        #region Table
        [Parameter]
        public string TableClass { get; set; }

        [Parameter]
        public Dictionary<string, object> TableAttributes { get; set; }


        private IEnumerable<KeyValuePair<string, object>> GetTableAttributes()
        {
            var TableAttrs = TableAttributes ?? Enumerable.Empty<KeyValuePair<string, object>>();
            if (TableClass != null)
            {
                TableAttrs = AddClass(TableAttrs, TableClass);
            }

            return AddStyle(TableAttrs, "overflow-anchor: none");
        }
        #endregion

        #region Head
        [Parameter]
        public string HeadClass { get; set; }

        [Parameter]
        public Dictionary<string, object> HeadAttributes { get; set; }


        private IEnumerable<KeyValuePair<string, object>> GetHeadAttributes()
        {
            var HeadAttrs = HeadAttributes ?? Enumerable.Empty<KeyValuePair<string, object>>();
            if (HeadClass != null)
            {
                HeadAttrs = AddClass(HeadAttrs, HeadClass);
            }

            return HeadAttrs;
        }
        #endregion
        #region Body
        [Parameter]
        public string BodyClass { get; set; }

        [Parameter]
        public Dictionary<string, object> BodyAttributes { get; set; }


        private IEnumerable<KeyValuePair<string, object>> GetBodyAttributes()
        {
            var BodyAttrs = BodyAttributes ?? Enumerable.Empty<KeyValuePair<string, object>>();
            if (BodyClass != null)
            {
                BodyAttrs = AddClass(BodyAttrs, BodyClass);
            }

            return AddStyle(BodyAttrs, "overflow-anchor: none");
        }
        #endregion


         
        private static IEnumerable<KeyValuePair<string, object>> AddClass(IEnumerable<KeyValuePair<string, object>> s, string c)
        {
            var found = false;
            foreach (var kv in s)
            {
                if (!found && "class".Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return new KeyValuePair<string, object>("class", kv.Value + " " + c);
                    found = true;
                }
                else
                {
                    yield return kv;
                }
            }
            if (!found)
            {
                yield return new KeyValuePair<string, object>("class", c);
            }
        }

        private static IEnumerable<KeyValuePair<string, object>> AddStyle(IEnumerable<KeyValuePair<string, object>> s, string c)
        {
            var found = false;
            foreach (var kv in s)
            {
                if (!found && "style".Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return new KeyValuePair<string, object>("style", kv.Value + ";" + c);
                    found = true;
                }
                else
                {
                    yield return kv;
                }
            }
            if (!found)
            {
                yield return new KeyValuePair<string, object>("style", c);
            }
        }


        protected override void RenderFirstPadding(RenderTreeBuilder builder, ref int sequence)
            => RenderPaddingCore(
                builder,
                ref sequence,
                0,
                FirstIndex - 1,
                Math.Max(0, (FirstIndex + ColumnCount - 1) / ColumnCount) * ItemHeight, tagName: "tr");

        protected override void RenderLastPadding(RenderTreeBuilder builder, ref int sequence)
            => RenderPaddingCore(
                builder,
                ref sequence,
                LastIndex + 1,
                Source.Count - 1,
                Math.Max(0, (Source.Count - LastIndex + ColumnCount - 2) / ColumnCount * ItemHeight), tagName: "tr");
    }
}
