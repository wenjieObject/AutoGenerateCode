using Common;
using IRepository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Repository;
using System;

namespace AutoGenerateCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            new Program().BuildServiceForSqlServer();

            //new Program().TestGenerateEntitiesForSqlServer();
            //Console.WriteLine("自动代码生成完成,按任意键退出");
            var dbContext = AspectCoreContainer.Resolve<IDbContextCore>();

            IFace_UserInfoRepository s = new Face_UserInfoRepository(dbContext);
            var model= s.GetSingle(1);
            var t= model.Guid_Id;


            Console.ReadLine();
        }

        public void TestGenerateEntitiesForSqlServer()
        {
            BuildServiceForSqlServer();
            CodeGenerator.GenerateAllCodesFromDatabase(true);
        }

        public IServiceProvider BuildServiceForSqlServer()
        {
            IServiceCollection services = new ServiceCollection();
            //在这里注册EF上下文
            services = RegisterSqlServerContext(services);
            services.Configure<CodeGenerateOption>(options =>
            {
                options.ModelsNamespace = "Models";
                options.IRepositoriesNamespace = "IRepository";
                options.RepositoriesNamespace = "Repository";
                options.OutputPath = "D:\\VSCode\\Demo\\AutoGenerateCode";
            });
            services.AddOptions();
            return AspectCoreContainer.BuildServiceProvider(services); //接入AspectCore.Injector

            

        }

        /// <summary>
        /// 注册SQLServer上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceCollection RegisterSqlServerContext(IServiceCollection services)
        {
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString =
                    "Data Source=(local);Initial Catalog=FaceDB;User ID=sa;pwd=123123123";
                options.ModelAssemblyName = "Models";
            });
            services.AddScoped<IDbContextCore, SqlServerDbContext>(); //注入EF上下文
            
            return services;
        }
    }




}
