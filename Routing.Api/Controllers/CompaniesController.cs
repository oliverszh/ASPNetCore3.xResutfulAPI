using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Net.Http.Headers;
using Routing.Api.ActionConstraints;
using Routing.Api.DtoParameters;
using Routing.Api.Entities;
using Routing.Api.Helpers;
using Routing.Api.Models;
using Routing.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Routing.Api.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
        public CompaniesController(ICompanyRepository companyRepository, IMapper mapper,IPropertyMappingService propertyMappingService,IPropertyCheckerService propertyCheckerService)
        {
            _companyRepository = companyRepository
                ?? throw new ArgumentNullException(nameof(companyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name =nameof(GetCompanies))]
        [HttpHead]
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyDtoParameters parameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<CompanyDto, Company>(parameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<CompanyDto>(parameters.Fields))
            {
                return BadRequest();
            }

            var companies = await _companyRepository.GetCompaniesAsync(parameters);
           

            //var previousPageLink = companies.HasPrevious ? CreateCompaniesResourceUri(parameters, ResourceUriType.PreviousPage) : null;
            //var nextPageLink = companies.HasNext ? CreateCompaniesResourceUri(parameters, ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = companies.TotalCount,
                pageSize = companies.PageSize,
                currentPage = companies.CurrentPage,
                totalPages = companies.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            //return Ok();
            //return companyDtos;
            var companyDtos = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            var shapedData = companyDtos.ShapeData(parameters.Fields);

            var links = CreateLinksForCompany(parameters,companies.HasPrevious,companies.HasNext);

            //{value:[xxx],links}

            var shapedCompaniesWithLinks = shapedData.Select(c =>
            {
                var companyDict = c as IDictionary<string, object>;
                var companyLinks = CreateLinksForCompany((Guid)companyDict["Id"], null);
                companyDict.Add("links", companyLinks);
                return companyDict;
            });

            var linkedCollectionResource = new
            {
                value = shapedCompaniesWithLinks,
                links
            };

            return Ok(linkedCollectionResource);
        }

        [Produces("application/json",
            "application/vnd.company.hateoas+json",
            "application/vnd.company.company.friendly+json",
            "application/vnd.company.company.friendly.hateoas+json",
            "application/vnd.company.company.full+json",
            "application/vnd.company.company.full.hateoas+json")]
        [HttpGet("{companyId}",Name =nameof(GetCompany))]        //api/companies/{companyId}
        //[Route("{companyId}")]
        public async Task<IActionResult> GetCompany(Guid companyId,string fields,[FromHeader(Name ="Accept")] string  mediaType)
        {
            if(!MediaTypeHeaderValue.TryParse(mediaType,out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<CompanyDto>(fields))
            {
                return BadRequest();
            }

            var company = await _companyRepository.GetCompanyAsync(companyId);
            
            if (company == null)
            {
                return NotFound();
            }

            var includeLinks = parsedMediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            IEnumerable<LinkDto> myLinks = new List<LinkDto>();

            if (includeLinks)  
            {
                myLinks = CreateLinksForCompany(companyId, fields);
            }

            var primaryMediaType = includeLinks ? parsedMediaType.SubTypeWithoutSuffix.Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - 8) : parsedMediaType.SubTypeWithoutSuffix;

            if (primaryMediaType == "vnd.company.company.full")
            {
                var full= _mapper.Map<CompanyFullDto>(company).ShapeData(fields) as IDictionary<string, object>;

                if (includeLinks)
                {
                    full.Add("links", myLinks);
                }
                return Ok(full);
            }

            var friendly = _mapper.Map<CompanyDto>(company).ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                friendly.Add("links", myLinks);
            }

            return Ok(friendly);

            //if (parsedMediaType.MediaType == "application/vnd.company.hateoas+json")
            //{
            //    var links = CreateLinksForCompany(companyId, fields);

            //    var LinkedDict = _mapper.Map<CompanyDto>(company).ShapeData(fields) as IDictionary<string, object>;

            //    LinkedDict.Add("links", links);

            //    return Ok(LinkedDict);
            //}

            //return Ok(_mapper.Map<CompanyDto>(company).ShapeData(fields));
            
        }

        [HttpPost(Name = nameof(CreateCompanyWithBankruptTime))]
        [RequestHeaderMatchesMediaType("Content-Type", "application/vnd.company.companyforcreationwithbankrupttime+json")]
        [Consumes("application/vnd.company.companyforcreationwithbankrupttime+json")]
        public async Task<ActionResult<CompanyDto>> CreateCompanyWithBankruptTime(CompanyAddWithBankruptTimeDto company)
        {
            //在3.0之前需要这样写
            //现在不需要了,因为当Controller使用[ApiController]属性进行注解的时候,如果遇到验证错误
            //那么就会自动返回400错误

            //if (!ModelState.IsValid)
            //{
            //    return UnprocessableEntity(ModelState);
            //}

            var entity = _mapper.Map<Company>(company);
            _companyRepository.AddCompany(entity);
            await _companyRepository.SaveAsync();

            var returnDto = _mapper.Map<CompanyDto>(entity);

            var links = CreateLinksForCompany(returnDto.Id, null);

            var linkedDict = returnDto.ShapeData(null) as IDictionary<string, object>;
            linkedDict.Add("links", links);

            return CreatedAtRoute(nameof(GetCompany), new { companyId = linkedDict["Id"] }, linkedDict);
        }



        [HttpPost(Name =nameof(CreateCompany))]
        [RequestHeaderMatchesMediaType("Content-Type","application/json",
            "application/vnd.company.companyforcreation+json")]
        [Consumes("application/json","application/vnd.company.companyforcreation+json")]
        public async Task<ActionResult<CompanyDto>> CreateCompany(CompanyAddDto company)
        {
            //在3.0之前需要这样写
            //现在不需要了,因为当Controller使用[ApiController]属性进行注解的时候,如果遇到验证错误
            //那么就会自动返回400错误

            //if (!ModelState.IsValid)
            //{
            //    return UnprocessableEntity(ModelState);
            //}

            var entity = _mapper.Map<Company>(company);
            _companyRepository.AddCompany(entity);
            await _companyRepository.SaveAsync();

            var returnDto = _mapper.Map<CompanyDto>(entity);

            var links = CreateLinksForCompany(returnDto.Id, null);

            var linkedDict = returnDto.ShapeData(null) as IDictionary<string, object>;
            linkedDict.Add("links", links);

            return CreatedAtRoute(nameof(GetCompany), new { companyId = linkedDict["Id"] }, linkedDict);
        }

        [HttpDelete("{companyId}",Name =nameof(DeleteCompany))]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var companyEntity = await _companyRepository.GetCompanyAsync(companyId);
            if (companyEntity == null)
            {
                return NotFound();
            }

            await _companyRepository.GetEmployeesAsync(companyId, null);

            _companyRepository.DeleteCompany(companyEntity);
            await _companyRepository.SaveAsync();
            return NoContent();
        }


        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET,POST,OPTIONS");
            return Ok();
        }


        private string CreateCompaniesResourceUri(CompanyDtoParameters parameters,ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        fields=parameters.Fields,
                        orderBy=parameters.OrderBy,
                        pageNumber = parameters.PageNumber - 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });
                case ResourceUriType.NextPage:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        fields = parameters.Fields,
                        orderBy = parameters.OrderBy,
                        pageNumber = parameters.PageNumber + 1,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });
                case ResourceUriType.CurrentPage:
                default:
                    return Url.Link(nameof(GetCompanies), new
                    {
                        fields = parameters.Fields,
                        orderBy = parameters.OrderBy,
                        pageNumber = parameters.PageNumber,
                        pageSize = parameters.PageSize,
                        companyName = parameters.CompanyName,
                        searchTerm = parameters.SearchTerm
                    });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForCompany(Guid companyId,string fields)
        {
            var links = new List<LinkDto>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(Url.Link(nameof(GetCompany), new { companyId }), "self", "GET"));
            }
            else
            {
                links.Add(new LinkDto(Url.Link(nameof(GetCompany), new { companyId,fields }), "self", "GET"));
            }

            links.Add(new LinkDto(Url.Link(nameof(DeleteCompany), new { companyId }), "delete_company", "DELETE"));

            links.Add(new LinkDto(Url.Link(nameof(EmployeesController.CreateEmployeeForCompany), new { companyId }),
                "create_employee_for_company",
                "POST"));

            links.Add(new LinkDto(Url.Link(nameof(EmployeesController.GetEmployeesForCompany),new { companyId}), "employees", "GET"));

            return links;
        }


        private IEnumerable<LinkDto> CreateLinksForCompany(CompanyDtoParameters parameters,bool hasPrevious,bool hasNext)
        {
            var linkList = new List<LinkDto>();

            linkList.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.CurrentPage), "self", "GET"));

            if (hasPrevious)
            {
                linkList.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.PreviousPage), "previous_page", "GET"));
            }

            if (hasNext)
            {
                linkList.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.NextPage), "next_page", "GET"));
            }

            return linkList;
        }

    }
}
