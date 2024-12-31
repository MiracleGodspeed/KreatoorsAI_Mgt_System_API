using KreatoorsAI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace KreatoorsAI.Data
{
    public class KreatoorsDbContext : DbContext
    {
        public KreatoorsDbContext(DbContextOptions<KreatoorsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(f => f.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            base.OnModelCreating(modelBuilder);

        }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
    }

}
