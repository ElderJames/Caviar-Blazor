﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Caviar.SharedKernel;
using Caviar.SharedKernel.Entities;
using Microsoft.AspNetCore.Identity;

namespace Caviar.Infrastructure.Identity
{
    public class ApplicationRole : IdentityRole<int>, IRole
    {
        [DisplayName("RoleName")]
        public override string Name { get => base.Name; set => base.Name = value; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [DisplayName("CreatTime")]
        public DateTime CreatTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改时间
        /// </summary>
        [DisplayName("UpdateTime")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 根据配置确定删除后是否保留条目
        /// </summary>
        [DisplayName("IsDelete")]
        public bool IsDelete { get; set; } = false;
        /// <summary>
        /// 创建操作员的名称
        /// </summary>
        [DisplayName("OperatorCare")]
        [StringLength(50, ErrorMessage = "LengthErrorMsg")]
        public string OperatorCare { get; set; }
        /// <summary>
        /// 创建操作员的名称
        /// </summary>
        [DisplayName("OperatorUp")]
        [StringLength(50, ErrorMessage = "LengthErrorMsg")]
        public string OperatorUp { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("Remark")]
        [StringLength(300, ErrorMessage = "LengthErrorMsg")]
        public string Remark { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        [DisplayName("IsDisable")]
        public bool IsDisable { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        [DisplayName("Number")]
        [StringLength(50, ErrorMessage = "LengthErrorMsg")]
        public string Number { get; set; } = "999";
        /// <summary>
        /// 数据权限
        /// </summary>
        [DisplayName("DataId")]
        public int DataId { get; set; }
        [DisplayName("ParentId")]
        public int ParentId { get; set; }
    }
}
