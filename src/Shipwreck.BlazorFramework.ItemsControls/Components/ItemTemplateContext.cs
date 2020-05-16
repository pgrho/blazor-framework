namespace Shipwreck.BlazorFramework.Components
{
    public sealed class ItemTemplateContext<T>
          where T : class
    {
        public ItemTemplateContext(int index, T item)
        {
            Index = index;
            Item = item;
        }

        public int Index { get; }
        public T Item { get; }

        public override bool Equals(object obj)
            => obj is ItemTemplateContext<T> other
            && other.Index == Index
            && other.Item == Item;

        public override string ToString()
            => Index + ": " + (Item?.ToString() ?? "{null}");

        public override int GetHashCode()
        {
            var h = Item?.GetHashCode() ?? 0;

            return Index ^ (h >> 16) ^ (h << 16);
        }
    }
}
