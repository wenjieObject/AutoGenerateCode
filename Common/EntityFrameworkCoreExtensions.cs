using Microsoft.EntityFrameworkCore;
using StackExchange.Redis.Extensions.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;

namespace Common
{
    public static class EntityFrameworkCoreExtensions
    {
        public static IList<DbTable> GetCurrentDatabaseTableList(this IDbContextCore context)
        {
            var tables = context.GetCurrentDatabaseAllTables().ToList<DbTable>();
            var db = context.GetDatabase();
            //仅支持SQL server
            //DatabaseType dbType;
            //if (db.IsSqlServer())
            //    dbType = DatabaseType.MSSQL;
            //else
            //{
            //    throw new NotImplementedException("This method does not support current database yet.");
            //}

            var dbType = DatabaseType.MSSQL;//仅支持SQL server 有时间补充其他数据库
            tables.ForEach(item =>
            {
                item.Columns = context.GetTableColumns(item.TableName).ToList<DbTableColumn>();
                item.Columns.ForEach(x =>
                {
                    var csharpType = DbColumnTypeCollection.DbColumnDataTypes.FirstOrDefault(t =>
                        t.DatabaseType == dbType && t.ColumnTypes.Split(',').Any(p =>
                            p.Trim().Equals(x.ColumnType, StringComparison.OrdinalIgnoreCase)))?.CSharpType;
                    if (string.IsNullOrEmpty(csharpType))
                    {
                        throw new SqlTypeException($"未从字典中找到\"{x.ColumnType}\"对应的C#数据类型，请更新DbColumnTypeCollection类型映射字典。");
                    }

                    x.CSharpType = csharpType;
                });
            });
            return tables;
        }

        /// <summary>
        /// 获取当前表的所有列和类型
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable GetTableColumns(this IDbContextCore context, string tableName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var db = context.GetDatabase();
            var sql = string.Empty;
            if (db.IsSqlServer())
            {
                sql = "SELECT a.name as ColName," +
                      "CONVERT(bit,(case when COLUMNPROPERTY(a.id,a.name,'IsIdentity')=1 then 1 else 0 end)) as IsIdentity, " +
                      "CONVERT(bit,(case when (SELECT count(*) FROM sysobjects  WHERE (name in (SELECT name FROM sysindexes  WHERE (id = a.id) AND (indid in  (SELECT indid FROM sysindexkeys  WHERE (id = a.id) AND (colid in  (SELECT colid FROM syscolumns WHERE (id = a.id) AND (name = a.name)))))))  AND (xtype = 'PK'))>0 then 1 else 0 end)) as IsPrimaryKey," +
                      "b.name as ColumnType," +
                      "COLUMNPROPERTY(a.id,a.name,'PRECISION') as ColumnLength," +
                      "CONVERT(bit,(case when a.isnullable=1 then 1 else 0 end)) as IsNullable,  " +
                      "isnull(e.text,'') as DefaultValue," +
                      "isnull(g.[value], ' ') AS Comments " +
                      "FROM  syscolumns a left join systypes b on a.xtype=b.xusertype  inner join sysobjects d on a.id=d.id and d.xtype='U' and d.name<>'dtproperties' left join syscomments e on a.cdefault=e.id  left join sys.extended_properties g on a.id=g.major_id AND a.colid=g.minor_id left join sys.extended_properties f on d.id=f.class and f.minor_id=0 " +
                      $"where b.name is not null and d.name='{tableName}' order by a.id,a.colorder";
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }

            return context.GetDataTable(sql);
        }



        /// <summary>
        /// 获取所有的表
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DataTable GetCurrentDatabaseAllTables(this IDbContextCore context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var db = context.GetDatabase();
            var sql = string.Empty;
            if (db.IsSqlServer())
            {
                sql = "select * from (SELECT (case when a.colorder=1 then d.name else '' end) as TableName," +
                      "(case when a.colorder=1 then isnull(f.value,'') else '' end) as TableComment" +
                      " FROM syscolumns a" +
                      " inner join sysobjects d on a.id=d.id  and d.xtype='U' and  d.name<>'dtproperties'" +
                      " left join sys.extended_properties f on d.id=f.major_id and f.minor_id=0) t" +
                      " where t.TableName!=''";
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }

            return context.GetDataTable(sql);
        }

        /// <summary>
        /// 拓展函数，执行sql获取表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(this IDbContextCore context, string sql, params DbParameter[] parameters)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var db = context.GetDatabase();
            db.EnsureCreated();
            var connection = db.GetDbConnection();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            var ds = new DataSet();
            var dt = new DataTable();
            DbCommand cmd;
            DataAdapter da;
            if (db.IsSqlServer())
            {
                cmd = new SqlCommand(sql, (SqlConnection)connection);
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                da = new SqlDataAdapter((SqlCommand)cmd);
            }
            else
            {
                throw new NotSupportedException("This method does not support current database yet.");
            }

            da.Fill(ds);
            dt = ds.Tables[0];
            da.Dispose();
            connection.Close();
            return dt;
        }

    }


    /// <summary>
    /// SQL server字段类型的对应关系
    /// </summary>
    public class DbColumnTypeCollection
    {
        public static IList<DbColumnDataType> DbColumnDataTypes => new List<DbColumnDataType>()
        {
            #region MSSQL，https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings

            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "bigint", CSharpType = "Int64"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "binary,image,varbinary(max),rowversion,timestamp,varbinary", CSharpType = "Byte[]"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "bit", CSharpType = "Boolean"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "char,nchar,text,ntext,varchar,nvarchar,xml", CSharpType = "String"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "date,datetime,datetime2,smalldatetime", CSharpType ="DateTime"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "datetimeoffset", CSharpType ="DateTimeOffset"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "decimal,money,numeric,smallmoney", CSharpType ="Decimal"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "float", CSharpType ="Double"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "int", CSharpType ="Int32"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "real", CSharpType ="Single"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "smallint", CSharpType ="Int16"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "sql_variant", CSharpType ="Object"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "time", CSharpType ="TimeSpan"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "tinyint", CSharpType ="Byte"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "uniqueidentifier", CSharpType ="Guid"},

            #endregion
        };

    }


    public class DbColumnDataType
    {
        public DatabaseType DatabaseType { get; set; }

        public string ColumnTypes { get; set; }

        public string CSharpType { get; set; }
    }

    public enum DatabaseType
    {
        MSSQL,
        MySQL,
        PostgreSQL,
        SQLite,
        InMemory,
        Oracle,
        MariaDB,
        MyCat,
        Firebird,
        DB2,
        Access
    }
}
