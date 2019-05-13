using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Common
{
    public class SqlServerDbContext: BaseDbContext, ISqlServerDbContext
    {
        public SqlServerDbContext(DbContextOption option) : base(option)
        {

        }
        public SqlServerDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        /// <summary>
        ///引入Microsoft.EntityFrameworkCore.Proxies包，启用延迟加载
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseSqlServer(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }


    }
}
