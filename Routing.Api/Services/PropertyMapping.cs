using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Server.IIS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Routing.Api.Services
{
    public class PropertyMapping<TSoruce,TDestination>:IPropertyMapping
    {
        public Dictionary<string,PropertyMappingValue> MappingDictionary { get; private set; }

        public PropertyMapping(Dictionary<string,PropertyMappingValue> mappingDictionary)
        {
            MappingDictionary = mappingDictionary ?? throw new ArgumentNullException(nameof(mappingDictionary));
        }
    }
}
