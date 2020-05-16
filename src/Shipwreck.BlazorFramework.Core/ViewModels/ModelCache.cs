using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Shipwreck.BlazorFramework.ViewModels
{
    public class ModelCache<TKey, TVersion>
    {
        private interface ITypeCache
        {
            int Count { get; }

            void Flush();
        }

        protected sealed class TypeCache<TModel> : ITypeCache
            where TModel : class, IVersionedModel<TKey, TVersion>
        {
            private readonly ModelCache<TKey, TVersion> _Cache;
            private readonly Dictionary<TKey, WeakReference<TModel>> _Dictionary;

            public TypeCache(ModelCache<TKey, TVersion> cache)
            {
                _Cache = cache;
                _Dictionary = new Dictionary<TKey, WeakReference<TModel>>();
            }

            public int Count => _Dictionary.Count;

            [TargetedPatchingOptOut("")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TModel GetByKey(TKey key)
                => _Cache.IsValidKey(key)
                && _Dictionary.TryGetValue(key, out var r)
                && r.TryGetTarget(out var e) ? e : null;

            [TargetedPatchingOptOut("")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IEnumerable<TModel> GetByKeys(IEnumerable<TKey> keys)
            {
                foreach (var k in keys)
                {
                    if (_Cache.IsValidKey(k))
                    {
                        var r = GetByKey(k);
                        if (r != null)
                        {
                            yield return r;
                        }
                    }
                }
            }

            [TargetedPatchingOptOut("")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TModel GetOrCreate<TParameter, TConverter>(TParameter parameter, TKey key, TConverter converter, object host)
                where TConverter : IModelConverter<TParameter, TModel>
            {
                if (_Cache.IsValidParameter(parameter))
                {
                    TModel d;

                    if ((_Cache.IsValidKey(key) || _Cache.IsValidKey(key = _Cache.GetKeyFromParameter(parameter)))
                        && _Dictionary.TryGetValue(key, out var dr)
                        && dr.TryGetTarget(out d))
                    {
                        d.Update(parameter);
                    }
                    else
                    {
                        d = converter.Convert(parameter, host);
                        if (d != null && _Cache.IsValidKey(d.Key))
                        {
                            _Dictionary[d.Key] = new WeakReference<TModel>(d);
                        }
                    }
                    return d;
                }
                else
                {
                    return GetByKey(key);
                }
            }

            [TargetedPatchingOptOut("")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public List<TModel> GetOrCreate<TParameter, TConverter>(IEnumerable<TParameter> parameters, TConverter converter, object host)
                  where TConverter : IModelConverter<TParameter, TModel>
            {
                var list = new List<TModel>();
                if (parameters != null)
                {
                    foreach (var e in parameters)
                    {
                        if (_Cache.IsValidParameter(e))
                        {
                            var r = GetOrCreate(e, default, converter, host);
                            if (r != null)
                            {
                                list.Add(r);
                            }
                        }
                    }
                }
                return list;
            }

            [TargetedPatchingOptOut("")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Attach(TModel entity)
            {
                if (entity != null)
                {
                    _Dictionary.Add(entity.Key, new WeakReference<TModel>(entity));
                }
            }

            public void Flush()
            {
                foreach (var kv in _Dictionary.ToArray())
                {
                    if (!kv.Value.TryGetTarget(out var t))
                    {
                        _Dictionary.Remove(kv.Key);
                    }
                }
            }
        }

        private Dictionary<Type, ITypeCache> _Caches;

        protected virtual bool IsValidKey(TKey key) => true;

        protected virtual bool IsValidParameter(object parameter) => parameter != null;

        protected virtual TKey GetKeyFromParameter(object parameter) => default;

        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TypeCache<TModel> GetCache<TModel>()
            where TModel : class, IVersionedModel<TKey, TVersion>
        {
            if (_Caches == null)
            {
                _Caches = new Dictionary<Type, ITypeCache>();
            }
            else if (_Caches.TryGetValue(typeof(TModel), out var obj) && obj is TypeCache<TModel> c)
            {
                return c;
            }
            var r = new TypeCache<TModel>(this);
            _Caches[typeof(TModel)] = r;
            return r;
        }

        [TargetedPatchingOptOut("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearCache<TModel>()
        {
            if (_Caches != null)
            {
                _Caches.Remove(typeof(TModel));
            }
        }

        public void Flush()
        {
            if (_Caches != null)
            {
                foreach (var kv in _Caches.ToArray())
                {
                    var oc = kv.Value.Count;
                    kv.Value.Flush();

                    if (kv.Value.Count <= 0)
                    {
                        _Caches.Remove(kv.Key);
                    }
                }
            }
        }
    }
}
