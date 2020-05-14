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

        #region ItemMarginX

        private float _ItemMarginX;

        [Parameter]
        public float ItemMarginX
        {
            get => _ItemMarginX;
            set => SetProperty(ref _ItemMarginX, value);
        }

        #endregion ItemMarginX

        #region ItemMarginY

        private float _ItemMarginY;

        [Parameter]
        public float ItemMarginY
        {
            get => _ItemMarginY;
            set => SetProperty(ref _ItemMarginY, value);
        }

        #endregion ItemMarginY

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

        protected bool SetRange(int first, int last)
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

                return true;
            }
            return false;
        }

        #endregion Range

        protected abstract bool HasClientSize { get; }

        protected abstract bool SetRange(ScrollInfo info, int firstIndex);

        protected abstract bool SetScroll(ItemsControlScrollInfo info);

        // protected float Local

        private bool _IsScrolling;

        protected abstract ValueTask ScrollAsync();

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
                    if (SetRange(si, Math.Max(FirstIndex, 0)))
                    {
                        _IsScrolling = true;
                        await ScrollAsync().ConfigureAwait(false);
                        _IsScrolling = false;
                    }
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

            if (SetRange(si, Math.Max(FirstIndex, 0)))
            {
                _IsScrolling = true;
                await ScrollAsync().ConfigureAwait(false);
                _IsScrolling = false;
            }
        }

        [JSInvokable]
        public async void OnElementScroll(string jsonScrollInfo)
        {
            if (_IsScrolling)
            {
                return;
            }

            var si = JsonSerializer.Deserialize<ItemsControlScrollInfo>(jsonScrollInfo);
            _MinItemWidth = si.MinWidth > 0 ? si?.MinWidth : null;
            _MinItemHeight = si.MinHeight > 0 ? si?.MinHeight : null;

            if (SetScroll(si))
            {
                _IsScrolling = true;
                await ScrollAsync().ConfigureAwait(false);
                _IsScrolling = false;
            }
        }

        public void Dispose()
            => Dispose(true);
    }
}
