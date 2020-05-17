using System;
using System.Collections.Generic;

namespace Shipwreck.BlazorFramework.Components
{
    public sealed class ComponentUpdateScope : IDisposable
    {
        internal interface IComponent
        {
            void EndUpdate(ComponentUpdateScope scope);
        }

        private readonly IComponent _Component;

        internal ComponentUpdateScope(IComponent component)
        {
            _Component = component;
        }

        public void Dispose()
            => _Component.EndUpdate(this);

        public static bool HasScopes(List<WeakReference<ComponentUpdateScope>> scopes, ComponentUpdateScope removingScope = null)
        {
            if (scopes != null)
            {
                for (var i = scopes.Count - 1; i >= 0; i--)
                {
                    if (!scopes[i].TryGetTarget(out var s) || (removingScope != null && s == removingScope))
                    {
                        scopes.RemoveAt(i);
                    }
                }
            }
            return scopes?.Count > 0;
        }
    }
}
