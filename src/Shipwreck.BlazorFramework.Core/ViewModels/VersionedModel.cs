using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Shipwreck.BlazorFramework.ViewModels
{
    public abstract class VersionedModel<TKey, TVersion> : StatefulModel, IVersionedModel<TKey, TVersion>
    {
        protected abstract TKey GetKey();

        TKey IVersionedModel<TKey, TVersion>.Key => GetKey();

        protected abstract TVersion GetVersion();

        TVersion IVersionedModel<TKey, TVersion>.Version => GetVersion();

        #region Update

        private WeakReference<object> _PreviousUpdate;
        private TVersion _PreviousUpdateVersion;

        protected abstract TVersion GetVersion(object parameter);

        protected virtual bool Equals(TVersion left, TVersion right)
            => object.Equals(left, right);

        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Update(object other)
        {
            if (_PreviousUpdate != null
                && _PreviousUpdate.TryGetTarget(out var pu)
                && pu == other
                && Equals(GetVersion(pu), _PreviousUpdateVersion))
            {
                return;
            }

            _PreviousUpdate = new WeakReference<object>(other);
            _PreviousUpdateVersion = GetVersion(other);

            UpdateCore(other);
        }

        void IVersionedModel<TKey, TVersion>.Update(object other) => Update(other);

        protected abstract void UpdateCore(object other);

        #endregion Update
    }
}
