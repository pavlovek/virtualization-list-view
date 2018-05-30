using System.Data.Entity;
using SamplesBasicDto;
using SamplesSpecificDto;

namespace SampleWpfApplication.Models
{
    public class HttpResponcesContext : DbContext
    {
        public HttpResponcesContext()
            : base("DbConnection")
        { }

        public DbSet<HttpResponce> HttpResponces { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HttpResponce>().Map<HttpTextResponce>(m => m.Requires("HttpResponceType").HasValue(1));
            modelBuilder.Entity<HttpResponce>().Map<HttpImageResponce>(m => m.Requires("HttpResponceType").HasValue(2));
            modelBuilder.Entity<HttpResponce>().Map<HttpVideoResponce>(m => m.Requires("HttpResponceType").HasValue(3));
        }
    }
}
