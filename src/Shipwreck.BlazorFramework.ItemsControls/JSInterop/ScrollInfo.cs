namespace Shipwreck.BlazorFramework.JSInterop
{
    public class ScrollInfo
    {
        public float ClientLeft { get; set; }
        public float ClientTop { get; set; }
        public float ClientWidth { get; set; }
        public float ClientHeight { get; set; }
        public float ScrollLeft { get; set; }
        public float ScrollTop { get; set; }
        public float ScrollWidth { get; set; }
        public float ScrollHeight { get; set; }

        public float ClientRight => ClientLeft + ClientWidth;
        public float ClientBottom => ClientTop + ClientHeight;

        public override string ToString()
            => $"{{{ClientLeft}-{ClientRight}, {ClientTop}-{ClientBottom}}} {{{ScrollLeft}, {ScrollTop}}}";
    }
}
