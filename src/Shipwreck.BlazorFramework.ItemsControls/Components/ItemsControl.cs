using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public abstract class ItemsControl<T> : ListComponentBase<T>, IDisposable, IScrollEventListener
          where T : class
    {
        private const string ITEM_INDEX_ATTRIBUTE = "data-itemindex";
        private const string ITEM_LAST_INDEX_ATTRIBUTE = "data-itemlastindex";

        public static string ItemIndexAttribute => ITEM_INDEX_ATTRIBUTE;
        public static string ItemLastIndexAttribute => ITEM_LAST_INDEX_ATTRIBUTE;

        [Inject]
        public IJSRuntime JS { get; set; }

        [Parameter]
        public string ItemSelector { get; set; } = ":scope > *[data-itemindex]";

        #region ItemTemplate

        private RenderFragment<ItemTemplateContext<T>> _ItemTemplate;

        [Parameter]
        public RenderFragment<ItemTemplateContext<T>> ItemTemplate
        {
            get => _ItemTemplate;
            set => SetProperty(ref _ItemTemplate, value);
        }

        #endregion ItemTemplate

        protected ElementReference Element { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object> AdditionalAttributes { get; set; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move && e.NewItems != null)
            {
                if (FirstIndex > 0
                    && e.NewStartingIndex + e.NewItems.Count < FirstIndex
                    && e.OldStartingIndex + e.NewItems.Count < FirstIndex)
                {
                    return;
                }
                if (LastIndex + 1 < Source?.Count
                    && e.NewStartingIndex + e.NewItems.Count > LastIndex
                    && e.OldStartingIndex + e.NewItems.Count > LastIndex)
                {
                    return;
                }
            }
            base.OnCollectionChanged(e);
        }

        protected override void OnItemAdded(T item)
        {
            base.OnItemAdded(item);
            CollectionChanged = true;
        }

        protected override void OnItemRemoved(T item)
        {
            base.OnItemRemoved(item);
            CollectionChanged = true;
        }

        protected override void OnReset()
        {
            base.OnReset();
            CollectionChanged = true;
        }

        protected override bool OnItemPropertyChanged(T item, string propertyName)
        {
            if (Source is IList list)
            {
                if (FirstIndex > 0 || LastIndex + 1 < Source.Count)
                {
                    var i = list.IndexOf(item);
                    if (i < FirstIndex || LastIndex < i)
                    {
                        return false;
                    }
                }
            }
            return base.OnItemPropertyChanged(item, propertyName);
        }

        #region CollectionChanged

        private bool _CollectionChanged;

        protected bool CollectionChanged
        {
            get => _CollectionChanged;
            private set => SetProperty(ref _CollectionChanged, value);
        }

        #endregion CollectionChanged

        #region Range

        private int _FirstIndex = -1;
        private int _LastIndex = -1;

        [Parameter]
        public int FirstIndex
        {
            get => _FirstIndex;
            set
            {
                var v = Math.Min(Math.Max(value, 0), (Source?.Count ?? 0) - 1);
                if (value != _FirstIndex
                    && Source?.Count > 0
                    && _LastIndex > _FirstIndex)
                {
                    _LastIndex += v - _FirstIndex;
                    _FirstIndex = v;

                    FirstIndexChanged?.Invoke(_FirstIndex);
                    LastIndexChanged?.Invoke(_LastIndex);

                    CollectionChanged = true;
                    StateHasChanged();
                }
            }
        }

        [Parameter]
        public Action<int> FirstIndexChanged { get; set; }

        [Parameter]
        public int LastIndex
        {
            get => _LastIndex;
            set
            {
                var v = Math.Min(Math.Max(value, 0), (Source?.Count ?? 0) - 1);
                if (v != _LastIndex
                    && Source?.Count > 0
                    && _LastIndex > _FirstIndex)
                {
                    _FirstIndex += v - _LastIndex;
                    _LastIndex = v;

                    FirstIndexChanged?.Invoke(_FirstIndex);
                    LastIndexChanged?.Invoke(_LastIndex);

                    CollectionChanged = true;

                    StateHasChanged();
                }
            }
        }

        [Parameter]
        public Action<int> LastIndexChanged { get; set; }

        protected abstract int ColumnCount { get; }

        protected async void SetVisibleRange(int first, int last, float localY, bool forceScroll)
        {
            first = Math.Max(0, first);
            last = Math.Min(last, Source?.Count ?? 0 - 1);
            if (last < 0 || first > last)
            {
                first = last = -1;
            }
            if (FirstIndex != first || LastIndex != last)
            {
                _FirstIndex = first;
                _LastIndex = last;

                FirstIndexChanged?.Invoke(_FirstIndex);
                LastIndexChanged?.Invoke(_LastIndex);

                StateHasChanged();
                if (forceScroll && first >= 0)
                {
                    await ScrollAsync(first, localY).ConfigureAwait(false);
                }
            }
            else if (forceScroll)
            {
                await ScrollAsync(first, localY).ConfigureAwait(false);
            }
        }

        #endregion Range

        #region MinimumRenderingCount

        private int _MinimumRenderingCount;

        [Parameter]
        public int MinimumRenderingCount
        {
            get => _MinimumRenderingCount;
            set => SetProperty(ref _MinimumRenderingCount, Math.Max(0, value));
        }

        #endregion

        protected abstract void SetScroll(ItemsControlScrollInfo info, bool forceScroll);

        protected abstract void UpdateRange(ScrollInfo info, int firstIndex, float localY, bool forceScroll);

        protected virtual ValueTask ScrollAsync(int firstIndex, float localY)
            => JS.scrollToItem(Element, ItemSelector, firstIndex, localY, ColumnCount, false);

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.AttachWindowResize(this).ConfigureAwait(false);
                await JS.AttachElementScroll(this, Element, ItemSelector).ConfigureAwait(false);
            }

            if (CollectionChanged)
            {
                _CollectionChanged = false;
                var si = await JS.GetScrollInfoAsync(Element).ConfigureAwait(false);

                UpdateRange(si, Math.Max(FirstIndex, 0), 0, true);
            }
        }

        [JSInvokable]
        public async void OnWindowResized()
        {
            try
            {
                var si = await JS.GetScrollInfoAsync(Element).ConfigureAwait(false);

                UpdateRange(si, Math.Max(FirstIndex, 0), 0, true);
            }
            catch { }
        }

        [JSInvokable]
        public void OnElementScroll(string jsonScrollInfo)
        {
            var si = JsonSerializer.Deserialize<ItemsControlScrollInfo>(jsonScrollInfo);

            SetScroll(si, false);
        }

        #region BuildRenderTree

        protected virtual string GetTagName() => "div";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;
            builder.OpenElement(sequence++, GetTagName());

            builder.AddMultipleAttributes(
                sequence++,
                RuntimeHelpers.TypeCheck<IEnumerable<KeyValuePair<string, object>>>(AdditionalAttributes));

            builder.AddElementReferenceCapture(sequence++, v => Element = v);

            sequence = RenderItemsAndPaddings(builder, ref sequence);

            builder.CloseElement();
        }

        protected virtual int RenderItemsAndPaddings(RenderTreeBuilder builder, ref int sequence)
        {
            if (Source != null)
            {
                var firstIndex = FirstIndex;
                var lastIndex = LastIndex;

                GetRenderingRange(ref firstIndex, ref lastIndex);

                RenderFirstPadding(builder, ref sequence, firstIndex);

                if (firstIndex >= 0)
                {
                    var li = Math.Min(lastIndex, Source.Count - 1);
                    for (var i = firstIndex; i <= li; i++)
                    {
                        builder.AddContent(sequence, ItemTemplate(new ItemTemplateContext<T>(i, Source[i])));
                    }
                }

                sequence++;

                RenderLastPadding(builder, ref sequence, lastIndex);
            }

            return sequence;
        }

        protected virtual void GetRenderingRange(ref int firstIndex, ref int lastIndex)
        {
            var rc = MinimumRenderingCount;
            if (Source?.Count > 0
                && rc > 0
                && (lastIndex - firstIndex + 1) < rc)
            {
                if (Source.Count <= rc)
                {
                    firstIndex = 0;
                    lastIndex = Source.Count - 1;
                }
                else if (0 <= firstIndex && firstIndex <= lastIndex)
                {
                    var bh = rc >> 1;
                    var ah = rc - bh;

                    var center = (firstIndex + lastIndex) >> 1;

                    if (center <= bh)
                    {
                        firstIndex = 0;
                        lastIndex = rc - 1;
                    }
                    else if (center + ah >= Source.Count - 1)
                    {
                        lastIndex = Source.Count - 1;
                        firstIndex = lastIndex - rc + 1;
                    }
                    else
                    {
                        firstIndex = center - bh;
                        lastIndex = center + ah;
                    }
                }
            }
        }

        protected static void RenderPaddingCore(RenderTreeBuilder builder, ref int sequence, int firstIndex, int lastIndex, float height, string tagName = "div")
        {
            builder.OpenElement(sequence++, tagName);
            if (height > 0)
            {
                builder.AddAttribute(sequence++, ITEM_INDEX_ATTRIBUTE, firstIndex);
                builder.AddAttribute(sequence++, ITEM_LAST_INDEX_ATTRIBUTE, lastIndex);
            }
            else
            {
                sequence += 2;
            }
            builder.AddAttribute(sequence++, "style", "margin:0;padding:0;width:100%;" + " height:" + Math.Max(0, height) + "px;opacity:0;overflow-anchor: none;");
            builder.CloseElement();
        }

        protected abstract void RenderFirstPadding(RenderTreeBuilder builder, ref int sequence, int firstIndex);

        protected abstract void RenderLastPadding(RenderTreeBuilder builder, ref int sequence, int lastIndex);

        #endregion BuildRenderTree

        #region IDisposable

        protected bool IsDisposed { get; set; }

        public void Dispose()
            => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                if (disposing && JS != null)
                {
                    JS.DetachWindowResize(this);
                }
            }
            IsDisposed = true;
        }

        #endregion IDisposable
    }
}
