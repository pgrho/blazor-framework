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

        protected int FirstIndex { get; private set; } = -1;
        protected int LastIndex { get; private set; } = -1;
        protected abstract int ColumnCount { get; }

        private bool _IsScrolling;

        protected async void SetVisibleRange(int first, int last, float localY)
        {
            first = Math.Max(0, first);
            last = Math.Min(last, Source?.Count ?? 0 - 1);
            if (last < 0 || first > last)
            {
                first = last = -1;
            }
            if (FirstIndex != first || LastIndex != last)
            {
                FirstIndex = first;
                LastIndex = last;

                _IsScrolling = true;
                StateHasChanged();
                if (first >= 0)
                {
                    await ScrollAsync(first, localY).ConfigureAwait(false);
                }
                _IsScrolling = false;
            }
        }

        #endregion Range

        protected abstract void SetScroll(ItemsControlScrollInfo info);

        protected abstract void UpdateRange(ScrollInfo info, int firstIndex, float localY);

        protected virtual ValueTask ScrollAsync(int firstIndex, float localY)
        {
            Console.WriteLine("ScrollAsync: #{0}+{1}px", firstIndex, localY);
            return JS.scrollToItem(Element, ItemSelector, firstIndex, localY, ColumnCount, false);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.AttachWindowResize(this).ConfigureAwait(false);
                await JS.AttachElementScroll(this, Element, ItemSelector).ConfigureAwait(false);

                if (CollectionChanged)
                {
                    _CollectionChanged = false;
                    var si = await JS.GetScrollInfoAsync(Element).ConfigureAwait(false);
                    UpdateRange(si, Math.Max(FirstIndex, 0), 0);
                }
            }
        }

        [JSInvokable]
        public async void OnWindowResized()
        {
            try
            {
                var si = await JS.GetScrollInfoAsync(Element).ConfigureAwait(false);
                UpdateRange(si, Math.Max(FirstIndex, 0), 0);
            }
            catch { }
        }

        [JSInvokable]
        public void OnElementScroll(string jsonScrollInfo)
        {
            if (_IsScrolling)
            {
                return;
            }

            var si = JsonSerializer.Deserialize<ItemsControlScrollInfo>(jsonScrollInfo);

            SetScroll(si);
        }

        #region BuildRenderTree

        protected virtual string GetTagName() => "div";

        protected static int RenderPaddingCoreSequence => 4;

        protected abstract int RenderFirstPaddingSequence { get; }

        protected abstract int RenderLastPaddingSequence { get; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;
            builder.OpenElement(sequence++, GetTagName());

            builder.AddMultipleAttributes(
                sequence++,
                RuntimeHelpers.TypeCheck<IEnumerable<KeyValuePair<string, object>>>(AdditionalAttributes));

            builder.AddElementReferenceCapture(sequence++, v => Element = v);

            if (Source != null)
            {
                if (ColumnCount > 0 && FirstIndex >= ColumnCount)
                {
                    RenderFirstPadding(builder, ref sequence);
                }
                else
                {
                    sequence = RenderFirstPaddingSequence;
                }

                if (FirstIndex >= 0)
                {
                    var li = Math.Min(LastIndex, Source.Count - 1);
                    for (var i = FirstIndex; i <= li; i++)
                    {
                        builder.AddContent(sequence, ItemTemplate(new ItemTemplateContext<T>(i, Source[i])));
                    }
                }

                sequence++;

                if (ColumnCount > 0 && LastIndex + ColumnCount < Source.Count)
                {
                    RenderLastPadding(builder, ref sequence);
                }
                else
                {
                    sequence = RenderLastPaddingSequence;
                }
            }

            builder.CloseElement();
        }

        protected static void RenderPaddingCore(RenderTreeBuilder builder, ref int sequence, int firstIndex, int lastIndex, float height, string tagName = "div")
        {
            builder.OpenElement(sequence++, tagName);
            builder.AddAttribute(sequence++, ITEM_INDEX_ATTRIBUTE, firstIndex);
            builder.AddAttribute(sequence++, ITEM_LAST_INDEX_ATTRIBUTE, lastIndex);
            builder.AddAttribute(sequence++, "style", "margin:0;padding:0;width:100%;" + " height:" + height + "px;opacity:0;");
            builder.CloseElement();
        }

        protected abstract void RenderFirstPadding(RenderTreeBuilder builder, ref int sequence);

        protected abstract void RenderLastPadding(RenderTreeBuilder builder, ref int sequence);

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
