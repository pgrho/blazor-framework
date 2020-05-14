using System;
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
        public string ItemSelector { get; set; } = ":scope > *[data-panelItemIndex]";

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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.AttachWindowResize(this).ConfigureAwait(false);
                await JS.AttachElementScroll(this, Element, ItemSelector).ConfigureAwait(false);
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
        public void OnWindowResized()
            => OnResized();

        [JSInvokable]
        public void OnElementScroll(string jsonPanelScrollInfo)
        {
            var si = JsonSerializer.Deserialize<ItemsControlScrollInfo>(jsonPanelScrollInfo);
            _MinItemWidth = si.MinWidth > 0 ? si?.MinWidth : null;
            _MinItemHeight = si.MinHeight > 0 ? si?.MinHeight : null;
            OnScrolled(si);
        }

        public virtual void OnResized()
        {
        }

        public virtual void OnScrolled(ItemsControlScrollInfo scrollInfo)
        {
        }

        public void Dispose()
            => Dispose(true);
    }
}
