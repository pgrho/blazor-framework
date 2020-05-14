using System;
using Shipwreck.BlazorFramework.JSInterop;

namespace Shipwreck.BlazorFramework.Components
{
    public partial class StackPanel<T> : ItemsControl<T>
        where T : class
    {
        public override async void OnResized()
        {
            var si = await JS.GetScrollInfoAsync(Element).ConfigureAwait(false);

            Console.WriteLine("OnWindowResized: {0}*{1}", si.ClientWidth, si.ClientHeight);
        }

        public override void OnScrolled(ItemsControlScrollInfo scrollInfo)
        {
            Console.WriteLine(
                "OnScrolled: {0} {1} {2} {3}",
                scrollInfo.Viewport,
                scrollInfo.First,
                scrollInfo.Last,
                scrollInfo.MinHeight);
        }
    }
}
