using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGenerateCode
{
    public interface IDbContextCore: IDisposable
    {
        DatabaseFacade GetDatabase();
    }
}
