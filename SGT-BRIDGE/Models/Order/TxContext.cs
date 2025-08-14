using Microsoft.EntityFrameworkCore;

namespace SGT_BRIDGE.Models.Order
{
    public class TxContext : DbContext
    {
        public TxContext(DbContextOptions<TxContext> options) : base(options) { }

        public DbSet<Tx> TxItems => Set<Tx>();
    }
}
