using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Business.StarSchema
{
    public class QueryCache
    {
        private readonly List<TableCache> _caches;

        public QueryCache()
        {
            _caches = new List<TableCache>();
        }

        public int GetId(string tableName, string value)
        {
            var cache = _caches.SingleOrDefault(c => c.TableName == tableName);
            var valueToRet = -1;
            if (cache == null)
                return valueToRet;
            cache.ValueId.TryGetValue(value, out valueToRet);
            return valueToRet;
        }

        public void Add(string tableName, string value, int id)
        {
            var cache = _caches.SingleOrDefault(c => c.TableName == tableName);
            if (cache == null)
            {
                cache = new TableCache(tableName);
                _caches.Add(cache);
            }
            cache.ValueId.Add(value, id);
        }

        private class TableCache
        {
            public TableCache(string tableName)
            {
                TableName = tableName;
                ValueId = new Dictionary<string, int>();
            }
            public string TableName { get; }
            public Dictionary<string, int> ValueId { get; }
        }
    }

    
}
