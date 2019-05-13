using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public abstract class BaseDbContext : DbContext, IDbContextCore
    {
        public DbContextOption Option { get; }

        public DatabaseFacade GetDatabase() => Database;

        public virtual DbSet<T> GetDbSet<T>() where T : class
        {
            return Set<T>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            MappingEntityTypes(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private void MappingEntityTypes(ModelBuilder modelBuilder)
        {
            if (string.IsNullOrEmpty(Option.ModelAssemblyName))
                return;
            var assembly = Assembly.Load(Option.ModelAssemblyName);
            var types = assembly?.GetTypes();
            //var list = types?.Where(t =>
            //    t.IsClass && !t.IsGenericType && !t.IsAbstract &&
            //    t.GetInterfaces().Any(m => m.IsAssignableFrom(typeof(BaseModel<>)))).ToList();
            var list = types?.Where(t =>
                t.IsClass && !t.IsGenericType && !t.IsAbstract && t.FullName.Substring(0,9)!= "BaseModel").ToList();
            if (list != null && list.Any())
            {
                list.ForEach(t =>
                {
                    if (modelBuilder.Model.FindEntityType(t) == null)
                        modelBuilder.Model.AddEntityType(t);
                });
            }

       
        }


        public virtual async Task<bool> EnsureCreatedAsync()
        {
            return await Database.EnsureCreatedAsync();
        }

        protected BaseDbContext(DbContextOption option)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="option"></param>
        protected BaseDbContext(IOptions<DbContextOption> option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));
            if (string.IsNullOrEmpty(option.Value.ConnectionString))
                throw new ArgumentNullException(nameof(option.Value.ConnectionString));
            Option = option.Value;
        }

    }
}
