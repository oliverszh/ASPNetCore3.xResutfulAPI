﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Models
{
    public class CompanyAddDto
    {
        [Required(ErrorMessage ="{0}这个字段是必填的"),Display(Name ="公司名"),MaxLength(100,ErrorMessage ="{0}的最大长度不可以超过{1}")]
        public string Name { get; set; }

        [Display(Name ="简介")]
        [StringLength(500,MinimumLength =10,ErrorMessage ="{0}的长度范围从{2}到{1}")]
        public string Introduction { get; set; }

        public ICollection<EmployeeAddDto> Employees { get; set; } = new List<EmployeeAddDto>();
    }
}
