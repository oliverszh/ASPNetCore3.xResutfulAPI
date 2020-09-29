using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Routing.Api.Entities;
using Routing.Api.Helpers;
using Routing.Api.Models;
using Routing.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Controllers
{
    [ApiController]
    [Route("api/companycollection")]
    public class CompanyCollectionsController:ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        public CompanyCollectionsController(IMapper mapper,ICompanyRepository companyRepository)
        {
            _mapper = mapper ?? throw new ArgumentException(nameof(mapper));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        }

        [HttpGet("({ids})",Name =nameof(GetCompanyCollection))]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanyCollection([FromRoute][ModelBinder(BinderType=typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }
            var companyEntities = await _companyRepository.GetCompaniesAsync(ids);
            if (companyEntities.Count() != ids.Count())
            {
                return NotFound();
            }
            var returnDtos = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            return Ok(returnDtos);
        }


        [HttpPost]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> CreateCompanyCollection(IEnumerable<CompanyAddDto> companyCollection)
        {
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var  company in companyEntities)
            {
                _companyRepository.AddCompany(company);
            }
            await _companyRepository.SaveAsync();
            var companyDtos = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            var idsString = string.Join(",", companyDtos.Select(x => x.Id));

            return CreatedAtRoute(nameof(GetCompanyCollection), new { ids = idsString }, companyDtos);
        }



    }
}
