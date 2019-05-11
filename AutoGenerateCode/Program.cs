using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoGenerateCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
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
                options.OutputPath = "D:\\VSCode\\AutoGenerateCode";
            });
            services.AddOptions();
            return AspectCoreContainer.BuildServiceProvider(services); //接入AspectCore.Injector

            return null;
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
            });
            services.AddScoped<IDbContextCore, SqlServerDbContext>(); //注入EF上下文
            return services;
        }
    }




}
