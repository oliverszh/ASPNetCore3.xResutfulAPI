using Routing.Api.Entities;
using Routing.Api.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Models
{
    [EmployeeNoMustDifferentFromFirstName(ErrorMessage = "员工名与编号必须不一样!!")]
    public abstract class EmployeeAddOrUpdateDto: IValidatableObject
    {
        [Display(Name = "员工号")]
        [Required(ErrorMessage = "{0}是必填的")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0}的长度是{1}")]
        public string EmployeeNo { get; set; }

        [Display(Name = "名")]
        [Required(ErrorMessage = "{0}是必填的")]
        [MaxLength(50, ErrorMessage = "{0}的长度不能超过{1}")]
        public string FirstName { get; set; }

        [Display(Name = "姓"), Required(ErrorMessage = "{0}是必填的"), MaxLength(50, ErrorMessage = "{0}的最大长度不能超过{1}")]
        public string LastName { get; set; }

        [Display(Name = "性别")]
        public Gender Gender { get; set; }

        [Display(Name = "出生日期")]
        public DateTime DateOfBirth { get; set; }

        //当DataAnnotations的验证有错误的时候(直接返回错误信息)，那么validate方法不会走的
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FirstName == LastName)
            {
                yield return new ValidationResult("姓和名不能一样", new[] { nameof(FirstName), nameof(LastName) });
            }
        }
    }
}
