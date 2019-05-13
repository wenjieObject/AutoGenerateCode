using System;
using Common;
using IRepository;
using Models;


namespace Repository
{
    public class Face_UserInfoRepository : BaseRepository<Face_UserInfo, Int32>, IFace_UserInfoRepository
    {
        public Face_UserInfoRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }
    }
}