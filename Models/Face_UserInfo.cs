using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	/// <summary>
	/// 
	/// </summary>
	[Table("Face_UserInfo")]
	public class Face_UserInfo:BaseModel<Int32>
	{
		/// <summary>
		/// 主键自增
		/// </summary>
		[Key]
		[Column("NumberId")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override Int32 Id {get;set;}


		/// <summary>
		/// 姓名
		/// </summary>
		[Column("UserName")]
		[MaxLength(50)]
		public String UserName {get;set;}


		/// <summary>
		/// 出生年月
		/// </summary>
		[Column("Month")]
		[MaxLength(50)]
		public String Month {get;set;}


		/// <summary>
		/// 性别
		/// </summary>
		[Column("Sex")]
		[MaxLength(50)]
		public String Sex {get;set;}


		/// <summary>
		/// 工作/学习单位
		/// </summary>
		[Column("Works")]
		[MaxLength(50)]
		public String Works {get;set;}


		/// <summary>
		/// 人脸唯一标识
		/// </summary>
		[Column("face_token")]
		[MaxLength(50)]
		public String face_token {get;set;}


		/// <summary>
		/// 人脸和数据库表关联字段
		/// </summary>
		[Column("Guid_Id")]
		[MaxLength(50)]
		public String Guid_Id {get;set;}



	}
}
