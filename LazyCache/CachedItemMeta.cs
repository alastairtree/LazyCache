using System;

namespace LazyCache
{
    public class CachedItemMeta
    {
        public CachedItemMeta()
        {
            this.CreatedDate = DateTime.UtcNow;
        }

        public DateTime CreatedDate { get; set; }
    }
}
