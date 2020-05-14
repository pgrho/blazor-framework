using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public abstract class ItemsControl<T> : ListComponentBase<T>, IDisposable, IScrollEventListener
          where T : class
    {
        protected bool IsDisposed { get; set; }

        [Inject]
        public IJSRuntime JS { get; set; }

        [Parameter]
        public string ItemSelector { get; set; } = ":scope > *[data-itemindex]";

        protected ElementReference Element { get; set; }

        //[Parameter(CaptureUnmatchedValues = true)]
        //public IDictionary<string, object> AdditionalAttributes { get; set; }

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

        protected abstract ValueTask ScrollAsync(int firstIndex, float localY);

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

        [JSInvokable]
        public async void OnWindowResized()
        {
            var si = await JS.GetScrollInfoAsync(Element).ConfigureAwait(false);
            UpdateRange(si, Math.Max(FirstIndex, 0), 0);
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

        public void Dispose()
            => Dispose(true);
    }
}
