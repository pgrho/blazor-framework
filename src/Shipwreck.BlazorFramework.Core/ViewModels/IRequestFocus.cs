using System;

namespace Shipwreck.BlazorFramework.ViewModels
{
    public interface IRequestFocus
    {
        event Action<string> FocusRequested;
    }
}
