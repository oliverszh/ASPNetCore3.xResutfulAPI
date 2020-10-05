using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Routing.Api.Models
{
    public class CompanyAddWithBankruptTimeDto:CompanyAddDto
    {
        public DateTime BankruptTime { get; set; }
    }
}
