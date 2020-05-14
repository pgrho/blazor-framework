namespace Shipwreck.BlazorFramework.JSInterop
{
    public sealed class ItemsControllElementInfo
    {
        public int FirstIndex { get; set; }
        public int LastIndex { get; set; }
        public float Left { get; set; }
        public float Top { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public float Right => Left + Width;
        public float Bottom => Top + Height;

        public override string ToString()
            => $"{FirstIndex}-{LastIndex} {{{Left}-{Right}, {Top}-{Bottom}}}";
    }
}
