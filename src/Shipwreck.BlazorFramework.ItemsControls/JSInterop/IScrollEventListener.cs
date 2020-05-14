namespace Shipwreck.BlazorFramework.JSInterop
{
    public interface IScrollEventListener : IWindowResizeEventListener
    {
        void OnElementScroll(string jsonScrollInfo);
    }
}
