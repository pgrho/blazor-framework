using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Shipwreck.BlazorFramework.JSInterop
{
    public static class JSRuntimeHelper
    {
        public static ValueTask AttachWindowResize(this IJSRuntime js, IWindowResizeEventListener listener)
            => js.InvokeVoidAsync(
                "Shipwreck.BlazorFramework.ItemsControls.attachWindowResize",
                DotNetObjectReference.Create(listener));

        public static ValueTask ScrollTo(this IJSRuntime js, ElementReference element, float left, float top, bool isSmooth)
            => js.InvokeVoidAsync(
                "Shipwreck.BlazorFramework.ItemsControls.scrollTo", element, left, top, isSmooth);

        public static ValueTask scrollToItem(this IJSRuntime js, ElementReference element, string itemSelector, int index, float localY, int column, bool isSmooth)
            => js.InvokeVoidAsync(
                "Shipwreck.BlazorFramework.ItemsControls.scrollToItem", element, itemSelector, index, localY, column, isSmooth);

        public static ValueTask DetachWindowResize(this IJSRuntime js, IWindowResizeEventListener listener)
            => js.InvokeVoidAsync(
                "Shipwreck.BlazorFramework.ItemsControls.detachWindowResize",
                DotNetObjectReference.Create(listener));

        public static ValueTask AttachElementScroll(this IJSRuntime js, IScrollEventListener listener, ElementReference element, string itemSelector)
            => js.InvokeVoidAsync(
                "Shipwreck.BlazorFramework.ItemsControls.attachElementScroll",
                element,
                DotNetObjectReference.Create(listener),
                itemSelector);

        public static async ValueTask<ScrollInfo> GetScrollInfoAsync(this IJSRuntime js, ElementReference element)
        {
            var json = await js.InvokeAsync<string>("Shipwreck.BlazorFramework.ItemsControls.getScrollInfo", element).ConfigureAwait(false);

            return JsonSerializer.Deserialize<ScrollInfo>(json);
        }

        public static async ValueTask<ItemsControlScrollInfo> GetItemsControlScrollInfoAsync(this IJSRuntime js, ElementReference element, string itemSelector)
        {
            var json = await js.InvokeAsync<string>("Shipwreck.BlazorFramework.ItemsControls.getItemsControlScrollInfo", element, itemSelector).ConfigureAwait(false);

            return JsonSerializer.Deserialize<ItemsControlScrollInfo>(json);
        }
    }
}
