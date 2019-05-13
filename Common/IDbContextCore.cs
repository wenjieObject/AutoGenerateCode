using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IDbContextCore: IDisposable
    {
        DatabaseFacade GetDatabase();

        DbSet<T> GetDbSet<T>() where T : class;

        Task<bool> EnsureCreatedAsync();


    }
}
