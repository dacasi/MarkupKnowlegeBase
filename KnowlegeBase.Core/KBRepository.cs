using System;
using System.IO;
using System.Collections.Generic;
using KnowlegeBase.Core.Model;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace KnowlegeBase.Core
{
    public class KBRepository : IKBRepository, IDisposable
    {
        private string _repositoryFile;
        private List<KBEntry> _entries;

        public KBRepository(string a_repositoryFile)
        {
            _repositoryFile = a_repositoryFile;
            if (_entries == null)
                Load();
        }

        public List<KBEntry> All
        {
            get  => _entries;
        }

        public void Add(KBEntry a_entry)
        {
            if (a_entry.ID == Guid.Empty) throw new ArgumentException("Entry must contain a valid Guid.");
            if (!_entries.Any(e => e.ID == a_entry.ID))
                return;
            _entries.Add(a_entry);
        }

        public void Remove(KBEntry a_entry)
        {
            _entries.RemoveAll(e => e.ID == a_entry.ID);
        }

        public void Save()
        {
            JsonConvert.SerializeObject(_entries, Formatting.Indented);
        }

        public void Update(KBEntry a_entry)
        {
            var entryToUpdate = _entries.FirstOrDefault(e => e.ID == a_entry.ID);
            if(entryToUpdate != null)
            {
                _entries.Remove(entryToUpdate);
                _entries.Add(a_entry);
            }
        }

        public void Dispose()  {  }

        private void Load()
        {
            try
            {
                if (File.Exists(_repositoryFile) && (new FileInfo(_repositoryFile)).Length > 0)
                {
                    var data = File.ReadAllText(_repositoryFile, Encoding.UTF8);
                    _entries = JsonConvert.DeserializeObject<List<KBEntry>>(data);
                }
                else
                {
                    _entries = new List<KBEntry>();
                }
            }
            catch
            {
                _entries = new List<KBEntry>();
            }
        }
    }
}
