using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Common;
using Microsoft.Extensions.Options;


namespace AutoGenerateCode
{
    public class CodeGenerator
    {
        private static readonly string Delimiter = "\\";//分隔符，默认为windows下的\\分隔符

        private static IOptions<CodeGenerateOption> options;

        /// <summary>
        /// 静态构造函数：从IoC容器读取配置参数，如果读取失败则会抛出ArgumentNullException异常
        /// </summary>
        static CodeGenerator()
        {
            options = AspectCoreContainer.Resolve<IOptions<CodeGenerateOption>>();
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var flag = path.IndexOf("/bin");
            if (flag > 0)
                Delimiter = "/";//如果可以取到值，修改分割符
        }

        /// <summary>
        /// 根据数据表生成Model层、Controller层、IRepository层和Repository层代码
        /// </summary>
        /// <param name="ifExsitedCovered">是否覆盖已存在的同名文件</param>
        public static void GenerateAllCodesFromDatabase(bool ifExsitedCovered = false)
        {
            var dbContext = AspectCoreContainer.Resolve<IDbContextCore>();
            if (dbContext == null)
                throw new Exception("未能获取到数据库上下文，请先注册数据库上下文。");
            var tables = dbContext.GetCurrentDatabaseTableList();
            if (tables != null && tables.Any())
            {
                foreach (var table in tables)
                {
                    if (table.Columns.Any(c => c.IsPrimaryKey))
                    {
                        var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                        GenerateEntity(table, ifExsitedCovered);
                        GenerateIRepository(table.TableName, pkTypeName, ifExsitedCovered);
                        GenerateRepository(table.TableName, pkTypeName, ifExsitedCovered);
                        //GenerateController(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                    }
                }
            }
        }



        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        private static void WriteAndSave(string fileName, string content)
        {
            //实例化一个文件流--->与写入文件相关联
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                //实例化一个StreamWriter-->与fs相关联
                using (var sw = new StreamWriter(fs))
                {
                    //开始写入
                    sw.Write(content);
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                }
            }
        }


        /// <summary>
        /// 从代码模板中读取内容
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        private static string ReadTemplate(string templateName)
        {
            var currentDic = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var currentDic1 = Environment.CurrentDirectory;
            var currentDic2 = AppDomain.CurrentDomain.BaseDirectory;

            var content = string.Empty;
            //获取不到txt文件流？ D:\VSCode\Czar.Cms\Czar.Cms.DataBase\CodeTemplate\ModelTemplate.txt
            string name = $"D:\\VSCode\\Demo\\AutoGenerateCode\\AutoGenerateCode\\CodeTemplate\\{templateName}";
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(name, false);
                content = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                sr.Close();
                return string.Empty;
            }
            sr.Close();
            return content;
        }


        /// <summary>
        /// 生成Repository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateRepository(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var repositoryPath = options.Value.OutputPath + Delimiter + "Repository";
            if (!Directory.Exists(repositoryPath))
            {
                Directory.CreateDirectory(repositoryPath);
            }
            var fullPath = repositoryPath + Delimiter + modelTypeName + "Repository.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("RepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", options.Value.IRepositoriesNamespace)
                .Replace("{RepositoriesNamespace}", options.Value.RepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }

        /// <summary>
        /// 生成IRepository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateIRepository(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var iRepositoryPath = options.Value.OutputPath + Delimiter + "IRepository";
            if (!Directory.Exists(iRepositoryPath))
            {
                Directory.CreateDirectory(iRepositoryPath);
            }
            var fullPath = iRepositoryPath + Delimiter + "I" + modelTypeName + "Repository.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("IRepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", options.Value.IRepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }

        #region 自动生成model

        /// <summary>
        /// 自动生成model
        /// </summary>
        /// <param name="table"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateEntity(DbTable table, bool ifExsitedCovered = false)
        {
            var modelPath = options.Value.OutputPath + Delimiter + "Models";
            if (!Directory.Exists(modelPath))
            {
                Directory.CreateDirectory(modelPath);
            }

            var fullPath = modelPath + Delimiter + table.TableName + ".cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;

            var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
            var sb = new StringBuilder();
            foreach (var column in table.Columns)
            {
                var tmp = GenerateEntityProperty(column);
                sb.AppendLine(tmp);
                sb.AppendLine();
            }
            var content = ReadTemplate("ModelTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{Comment}", table.TableComment)
                .Replace("{TableName}", table.TableName)
                .Replace("{ModelName}", table.TableName)
                .Replace("{KeyTypeName}", pkTypeName)
                .Replace("{ModelProperties}", sb.ToString());
            WriteAndSave(fullPath, content);
        }
        /// <summary>
        /// 自动生成model的属性
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private static string GenerateEntityProperty(DbTableColumn column)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(column.Comments))
            {
                sb.AppendLine("\t\t/// <summary>");
                sb.AppendLine("\t\t/// " + column.Comments);
                sb.AppendLine("\t\t/// </summary>");
            }
            if (column.IsPrimaryKey)
            {
                sb.AppendLine("\t\t[Key]");
                sb.AppendLine($"\t\t[Column(\"{column.ColName}\")]");
                if (column.IsIdentity)
                {
                    sb.AppendLine("\t\t[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }
                sb.AppendLine($"\t\tpublic override {column.CSharpType} Id " + "{get;set;}");
            }
            else
            {
                sb.AppendLine($"\t\t[Column(\"{column.ColName}\")]");
                if (!column.IsNullable)
                {
                    sb.AppendLine("\t\t[Required]");
                }

                if (column.ColumnLength.HasValue && column.ColumnLength.Value > 0)
                {
                    sb.AppendLine($"\t\t[MaxLength({column.ColumnLength.Value})]");
                }
                if (column.IsIdentity)
                {
                    sb.AppendLine("\t\t[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }

                var colType = column.CSharpType;
                if (colType.ToLower() != "string" && colType.ToLower() != "byte[]" && colType.ToLower() != "object" &&
                    column.IsNullable)
                {
                    colType = colType + "?";
                }

                sb.AppendLine($"\t\tpublic {colType} {column.ColName} " + "{get;set;}");
            }

            return sb.ToString();
        }

        #endregion
    }
}
