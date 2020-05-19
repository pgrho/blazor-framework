using System.Collections.Generic;

namespace Shipwreck.BlazorFramework.Components
{
    public class ScrollableTableTheme
    {
        public string HeaderHeight { get; set; }

        public Dictionary<string, object> ElementAttributes { get; set; }
        public string ElementClass { get; set; }

        public string HeaderBackgroundClass { get; set; }
        public string HeaderBackgroundBackground { get; set; }
        public Dictionary<string, object> HeaderBackgroundAttributes { get; set; }

        public string ScrollerClass { get; set; }

        public Dictionary<string, object> ScrollerAttributes { get; set; }

        public string TableClass { get; set; }

        public Dictionary<string, object> TableAttributes { get; set; }

        public string TableHeadClass { get; set; }

        public Dictionary<string, object> TableHeadAttributes { get; set; }

        public string TableBodyClass { get; set; }
        public Dictionary<string, object> TableBodyAttributes { get; set; }

        public IDictionary<string, object> TableHeaderCellAttributes { get; set; }

        public IDictionary<string, object> TableHeaderCellInnerAttributes { get; set; }
    }
}
