using Microsoft.EntityFrameworkCore;

namespace SGT_BRIDGE.Models
{
    public class TxContext : DbContext
    {
        public TxContext(DbContextOptions<TxContext> options) : base(options) { }

        public DbSet<Tx> TxItems => Set<Tx>();
    }
}
