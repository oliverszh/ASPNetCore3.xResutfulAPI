using Routing.Api.DtoParameters;
using Routing.Api.Entities;
using Routing.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Services
{
    public interface ICompanyRepository
    {
        Task<PageList<Company>> GetCompaniesAsync(CompanyDtoParameters parameters);

        Task<Company> GetCompanyAsync(Guid companyId);

        Task<IEnumerable<Company>> GetCompaniesAsync(IEnumerable<Guid> companyIds);

        void AddCompany(Company company);

        void UpdateCompany(Company company);

        void DeleteCompany(Company company);

        Task<bool> CompanyExistsAsync(Guid companyId);

        Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId,EmployeeDtoParameters parameters);

        Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId);

        void AddEmployee(Guid companyId, Employee employee);

        void UpdateEmployee(Employee employee);

        void DeleteEmployee(Employee employee);

        Task<bool> SaveAsync();
    }
}
