using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGenerateCode
{
    public abstract class BaseDbContext : DbContext, IDbContextCore
    {

    }
}
