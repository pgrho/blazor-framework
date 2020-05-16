namespace Shipwreck.BlazorFramework.ViewModels
{
    public interface IModelConverter<TParameter, TResult>
    {
        TResult Convert(TParameter parameter, object host);
    }
}
