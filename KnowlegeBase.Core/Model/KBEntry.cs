using System;
using System.Collections.Generic;
using System.Text;

namespace KnowlegeBase.Core.Model
{
    public class KBEntry
    {
        public Guid ID { get; set; }
        public List<string> Tags { get; set; }
        public string Url { get; set; }
    }
}
