using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipwreck.BlazorFramework.Components;
using Shipwreck.BlazorFramework.ViewModels;

namespace Shipwreck.BlazorFramework.Demo.Pages.ItemsControls
{
    public abstract class ItemsControlPage : BindableComponentBase
    {
        private sealed class ItemCollection : VirtualizedCollection<string>
        {
            private string[] _Items;

            public ItemCollection(IEnumerable<string> items)
            {
                Items = items.ToArray();
            }

            public string[] Items
            {
                get => _Items;
                set
                {
                    if (SetProperty(ref _Items, value))
                    {
                        Invalidate();
                    }
                }
            }

            protected override Task<PageResult> SearchAsync(int offset)
                => Task.Delay(1000).ContinueWith(t => new PageResult(offset, Items.Length, Items.Skip(offset).Take(PageSize)));
        }

        public int Count { get; set; } = 500;

        public bool IsVirtualized { get; set; } = true;

        public IReadOnlyList<string> Apply()
        {
            if (IsVirtualized)
            {
                if (_Items is ItemCollection c)
                {
                    c.Items = GetData().ToArray();
                }
                else
                {
                    _Items = c = new ItemCollection(GetData());
                }
                return c;
            }
            else
            {
                return _Items = Array.AsReadOnly(GetData().ToArray());
            }
        }

        #region Items

        private IReadOnlyList<string> _Items;

        public IReadOnlyList<string> Items
        {
            get => _Items ?? (_Items = Apply());
        }

        #endregion Items

        #region MyRegion

        private int _FirstIndex;

        protected int FirstIndex
        {
            get => _FirstIndex;
            set
            {
                if (value != _FirstIndex)
                {
                    NewFirstIndex = _FirstIndex = value;
                    StateHasChanged();
                }
            }
        }

        protected int NewFirstIndex { get; set; }

        public void SetFirstIndex()
        {
            if (NewFirstIndex != _FirstIndex)
            {
                _FirstIndex = NewFirstIndex;
                StateHasChanged();
            }
        }

        #endregion MyRegion

        private IEnumerable<string> GetData()
            => Enumerable.Range(0, Count).Select(e => e.ToString("x4"));
    }
}
