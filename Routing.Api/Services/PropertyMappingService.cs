﻿using Routing.Api.Entities;
using Routing.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> companyMappingService = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            {"Id",new PropertyMappingValue(new List<string>{"Id"}) },
            {"CompanyName",new PropertyMappingValue(new List<string>{ "Name"}) },
            {"Country",new PropertyMappingValue(new List<string>{ "Country" })},
            {"Industry",new PropertyMappingValue(new List<string>{ "Industry"})},
            {"Product",new PropertyMappingValue(new List<string>{"Product" }) },
            {"Introduction",new PropertyMappingValue(new List<string>{ "Introduction"}) }
        };

        private readonly Dictionary<string, PropertyMappingValue> employeeMappingService = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            {"Id",new PropertyMappingValue(new List<string>{"Id"}) },
            {"CompanyId",new PropertyMappingValue(new List<string>{ "CompanyId"}) },
            {"EmployeeNo",new PropertyMappingValue(new List<string>{ "EmployeeNo" })},
            {"Name",new PropertyMappingValue(new List<string>{ "FirstName","LastName"})},
            {"GenderDisplay",new PropertyMappingValue(new List<string>{"Gender" }) },
            {"Age",new PropertyMappingValue(new List<string>{ "DateOfBirth"},true) }
        };

        private IList<IPropertyMapping> propertyMappings=new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<EmployeeDto, Employee>(employeeMappingService));
            propertyMappings.Add(new PropertyMapping<CompanyDto, Company>(companyMappingService));
        }

        public Dictionary<string,PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
            var propertyMapping = matchingMapping.ToList();
            if (propertyMapping.Count == 1)
            {
                return matchingMapping.First().MappingDictionary;
            } 

            throw new Exception($"无法找到唯一的映射关系：{typeof(TSource)},{typeof(TDestination)}");
        }

        public bool ValidMappingExistsFor<TSource,TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }
            var fieldAfterSplit = fields.Split(",");
            foreach (var field in fieldAfterSplit)
            {
                var trimmedField = field.Trim();
                var indexOfFirstSpace = trimmedField.IndexOf(" ",StringComparison.Ordinal);
                var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
