using Microsoft.EntityFrameworkCore;
using System;

namespace DBServices
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Deal> Deals { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
