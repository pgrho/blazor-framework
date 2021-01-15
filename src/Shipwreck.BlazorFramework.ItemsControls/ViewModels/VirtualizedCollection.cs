using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.BlazorFramework.ViewModels
{
    public abstract class VirtualizedCollection<T> : ObservableModel, IList<T>, IReadOnlyList<T>, IList, INotifyCollectionChanged
        where T : class
    {
        public sealed class PageResult
        {
            public PageResult(int offset, int totalCount, IEnumerable<T> items)
            {
                Offset = offset;
                TotalCount = totalCount;
                Items = Array.AsReadOnly(items.ToArray());
            }

            public int Offset { get; }
            public int TotalCount { get; }
            public ReadOnlyCollection<T> Items { get; }
        }

        private sealed class PageState
        {
            public PageState(int offset, Task<PageResult> task)
            {
                Offset = offset;
                Task = task;
            }

            public int Offset { get; }

            public Task<PageResult> Task { get; }
        }

        protected VirtualizedCollection(int pageSize = 50, int maxPageCount = 16, int pageIndex = 0)
        {
            _PageSize = AssertPageSize(pageSize, nameof(pageSize));
            _MaxPageCount = AssertMaxPageCount(maxPageCount, nameof(maxPageCount));
            _PageIndex = AssertPageIndex(pageIndex, nameof(pageIndex));
            SyncRoot = new object();
            _Pages = new List<PageState>();
        }

        private readonly List<PageState> _Pages;

        /// <summary>Increments on totalCount changes</summary>
        private uint _Version;

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    foreach (var p in _Pages)
                    {
                        if (p.Task.Status == TaskStatus.RanToCompletion)
                        {
                            return p.Task.Result.TotalCount;
                        }
                    }

                    if (!_Pages.Any())
                    {
                        BeginLoadPage(PageIndex * PageSize);
                    }

                    return 0;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                var offset = (index / PageSize) * PageSize;

                lock (SyncRoot)
                {
                    var valid = false;
                    foreach (var p in _Pages)
                    {
                        if (p.Task.Status == TaskStatus.RanToCompletion)
                        {
                            var r = p.Task.Result;

                            if (p.Offset == offset)
                            {
                                return r.Items[index - offset];
                            }
                            else if (index >= r.TotalCount)
                            {
                                throw new IndexOutOfRangeException();
                            }
                            else
                            {
                                valid = true;
                            }
                        }
                    }
                    BeginLoadPage(offset);
                    if (!valid)
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
                return null;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region PageIndex

        private int _PageIndex;

        protected int PageIndex
        {
            get => _PageIndex;
            set => _PageIndex = AssertPageIndex(value);
        }

        private static int AssertPageIndex(int value, string parameterName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName ?? "value");
            }
            return value;
        }

        #endregion PageIndex

        #region PageSize

        private int _PageSize;

        public int PageSize
        {
            get => _PageSize;
            set
            {
                if (SetProperty(ref _PageSize, AssertPageSize(value)))
                {
                    Invalidate();
                }
            }
        }

        private static int AssertPageSize(int value, string parameterName = null)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(parameterName ?? "value");
            }
            return value;
        }

        #endregion PageSize

        #region MaxPageCount

        private int _MaxPageCount;

        public int MaxPageCount
        {
            get => _MaxPageCount;
            set
            {
                var cv = _MaxPageCount;
                if (SetProperty(ref _MaxPageCount, AssertMaxPageCount(value))
                    && cv > _MaxPageCount)
                {
                    lock (SyncRoot)
                    {
                        var newPageCount = 0;
                        for (var i = _Pages.Count - 1; i >= 0; i--)
                        {
                            var oldPage = _Pages[i];

                            if (oldPage.Task.Status == TaskStatus.RanToCompletion)
                            {
                                if (newPageCount < MaxPageCount)
                                {
                                    newPageCount++;
                                }
                                else
                                {
                                    _Pages.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int AssertMaxPageCount(int value, string parameterName = null)
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(parameterName ?? "value");
            }
            return value;
        }

        #endregion MaxPageCount

        #region SearchingPageCount

        private int _SearchingPageCount;

        public int SearchingPageCount
        {
            get => _SearchingPageCount;
            private set => SetProperty(ref _SearchingPageCount, value);
        }

        private void SetSearchingPageCount()
        {
            lock (SyncRoot)
            {
                int c = 0;

                foreach (var p in _Pages)
                {
                    if (!p.Task.IsCompleted && !p.Task.IsFaulted && !p.Task.IsCanceled)
                    {
                        c++;
                    }
                }
                SearchingPageCount = c;
            }
        }

        #endregion SearchingPageCount

        private async void BeginLoadPage(int offset)
        {
            PageState p;
            lock (SyncRoot)
            {
                foreach (var e in _Pages)
                {
                    if (e.Offset == offset && !e.Task.IsFaulted)
                    {
                        return;
                    }
                    else if (e.Task.Status == TaskStatus.RanToCompletion)
                    {
                        if (offset >= e.Task.Result.TotalCount)
                        {
                            throw new ArgumentException();
                        }
                    }
                }
                p = new PageState(offset, SearchAsync(offset));
                _Pages.Add(p);
                SetSearchingPageCount();
            }

            try
            {
                var r = await p.Task.ConfigureAwait(false);

                if (r.TotalCount > 0 && r.TotalCount <= r.Offset)
                {
                    BeginLoadPage((r.TotalCount - 1) / PageSize);
                }

                lock (SyncRoot)
                {
                    var pi = _Pages.IndexOf(p);
                    if (pi < 0)
                    {
                        return;
                    }

                    var newPageCount = 0;
                    var empty = true;
                    var pv = _Version;
                    PageResult oldResult = null;
                    for (var i = _Pages.Count - 1; i >= 0; i--)
                    {
                        var oldPage = _Pages[i];
                        if (oldPage == p)
                        {
                            newPageCount++;
                        }
                        else
                        {
                            if (oldPage.Task.Status == TaskStatus.RanToCompletion)
                            {
                                if (ShouldInvalidate(r, oldPage.Task.Result))
                                {
                                    _Pages.RemoveAt(i);
                                    _Version++;
                                    if (oldPage.Offset == r.Offset)
                                    {
                                        oldResult = oldPage.Task.Result;
                                    }
                                }
                                else if (newPageCount < MaxPageCount)
                                {
                                    newPageCount++;
                                }
                                else
                                {
                                    _Pages.RemoveAt(i);
                                }
                                empty = false;
                            }
                        }
                    }
                    if (empty)
                    {
                        _Version++;
                    }

                    if (_Version != pv)
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else if (r.Items.Any())
                    {
                        // Notify page insersion
                        NotifyPageUpdate(r, oldResult);
                    }
                    else if (empty)
                    {
                        // No result but TotalCount changed
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                }
            }
            catch
            {
                lock (SyncRoot)
                {
                    _Pages.Remove(p);
                }
            }
            finally
            {
                SetSearchingPageCount();
            }
        }

        protected virtual void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void NotifyPageUpdate(PageResult newResult, PageResult oldResult)
        {
            RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    newResult.Items,
                    oldResult?.Items ?? (IList)new T[newResult.Items.Count],
                    newResult.Offset));
        }

        protected virtual bool ShouldInvalidate(PageResult newResult, PageResult oldResult)
            => oldResult.Offset == newResult.Offset
                || oldResult.TotalCount != newResult.TotalCount;

        protected bool Invalidate()
        {
            lock (SyncRoot)
            {
                if (_Pages.Any())
                {
                    _Pages.Clear();

                    _Version++;

                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                    SetSearchingPageCount();

                    return true;
                }
            }

            return false;
        }

        public KeyValuePair<int, T>[] GetLoadedItems()
        {
            lock (SyncRoot)
            {
                var pl = _Pages.Where(e => e.Task.IsCompletedSuccessfully).OrderBy(e => e.Offset).ToList();
                var tc = pl.Sum(e => e.Task.Result.Items.Count);
                var r = new KeyValuePair<int, T>[tc];
                var i = 0;
                foreach (var p in pl)
                {
                    var oi = p.Offset;
                    foreach (var e in p.Task.Result.Items)
                    {
                        r[i++] = new KeyValuePair<int, T>(oi++, e);
                    }
                }

                return r;
            }
        }

        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator()
        {
            List<PageState> pages;
            uint v;
            lock (SyncRoot)
            {
                if (_Pages.Count == 0)
                {
                    BeginLoadPage(PageIndex * PageSize);
                    yield break;
                }

                pages = _Pages.Where(e => e.Task.Status == TaskStatus.RanToCompletion).OrderBy(e => e.Offset).ToList();
                v = _Version;
            }

            if (pages.Any())
            {
                var pi = 0;
                var returned = 0;
                var totalCount = pages[0].Task.Result.TotalCount;

                while (returned < totalCount)
                {
                    if (_Version != v)
                    {
                        throw new InvalidOperationException();
                    }

                    if (pi < pages.Count)
                    {
                        var p = pages[pi];
                        if (p.Offset == returned)
                        {
                            foreach (var e in p.Task.Result.Items)
                            {
                                if (_Version != v)
                                {
                                    throw new InvalidOperationException();
                                }

                                yield return e;
                                returned++;
                            }
                            pi++;
                        }
                    }

                    yield return null;
                    returned++;
                }
            }
            else if (_Pages.Count == 0)
            {
                BeginLoadPage(PageIndex * PageSize);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable<T>

        #region ICollection<T>

        bool ICollection<T>.IsReadOnly => true;

        void ICollection<T>.Clear()
            => throw new NotSupportedException();

        void ICollection<T>.Add(T item)
            => throw new NotSupportedException();

        bool ICollection<T>.Remove(T item) => false;

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            => CopyToCore(array, arrayIndex);

        bool ICollection<T>.Contains(T item) => IndexOfCore(item) >= 0;

        #endregion ICollection<T>

        #region IList<T>

        T IList<T>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        int IList<T>.IndexOf(T item) => IndexOfCore(item);

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

        #endregion IList<T>

        #region ICollection

        public object SyncRoot { get; }

        bool ICollection.IsSynchronized => false;

        void ICollection.CopyTo(Array array, int index)
            => CopyToCore((T[])array, index);

        #endregion ICollection

        #region IList

        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => false;

        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        int IList.Add(object value) => throw new NotSupportedException();

        void IList.Clear() => throw new NotSupportedException();

        void IList.Insert(int index, object value) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        bool IList.Contains(object value) => value is T e && IndexOfCore(e) >= 0;

        int IList.IndexOf(object value) => value is T e ? IndexOfCore(e) : -1;

        #endregion IList

        private void CopyToCore(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                var first = true;
                foreach (var p in _Pages)
                {
                    if (p.Task.Status == TaskStatus.RanToCompletion)
                    {
                        var r = p.Task.Result;
                        if (first)
                        {
                            if (r.TotalCount <= 0)
                            {
                                return;
                            }

                            Array.Clear(array, arrayIndex, r.TotalCount);

                            first = false;
                        }

                        if (r.Items.Any())
                        {
                            r.Items.CopyTo(array, arrayIndex + r.Offset);
                        }
                    }
                }
            }
        }

        private int IndexOfCore(T item)
        {
            if (item != null)
            {
                lock (SyncRoot)
                {
                    foreach (var p in _Pages)
                    {
                        if (p.Task.Status == TaskStatus.RanToCompletion)
                        {
                            var r = p.Task.Result;
                            var i = r.Items.IndexOf(item);
                            if (i >= 0)
                            {
                                return r.Offset + i;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        protected abstract Task<PageResult> SearchAsync(int offset);
    }
}
