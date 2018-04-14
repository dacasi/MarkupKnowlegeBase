using KnowlegeBase.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace KnowlegeBase.Core
{
    public interface IKBRepository
    {
        List<KBEntry> All { get; }
        void Add(KBEntry a_entry);
        void Remove(KBEntry a_entry);
        void Update(KBEntry a_entry);
        void Save();
    }
}
