using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IRepository
{
    public interface  IBaseRepository<T,TKey> : IDisposable where T : BaseModel<TKey>
    {
        #region 同步方法

        /// <summary>
        /// 通过主键获取实体对象
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        T GetSingle(TKey key);

        #endregion
    }
}
