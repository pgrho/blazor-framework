namespace Shipwreck.BlazorFramework.ViewModels
{
    public interface IVersionedModel<TKey, TVersion>
    {
        TKey Key { get; }
        TVersion Version { get; }

        void Update(object other);
    }
}
