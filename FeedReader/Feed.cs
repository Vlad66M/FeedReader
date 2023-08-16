using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedReader
{
    internal class Feed
    {
        public string Source { get; set; }
        public string SourceName{ get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int IsRead { get; set; }
        public int IsDeleted { get; set; }
        public int IsTrullyDeleted { get; set; }
        public DateTime? Date { get; set; }
    }

    internal class FeedSource
    {
        public string Source { get; set; }
        public string SourceName { get; set; }
    }
}
