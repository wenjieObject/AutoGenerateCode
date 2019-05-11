using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGenerateCode
{
    public abstract class BaseDbContext : DbContext, IDbContextCore
    {
        public DatabaseFacade GetDatabase() => Database;

    }
}
