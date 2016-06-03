using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Web;

namespace WcfCrimShopService.entities
{
    public class TypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat
            GetMessageFormatForContentType(string contentType)
        {
            if (contentType == "text/plain; charset=utf-8")
            {
                return WebContentFormat.Json;
            }
            else
            {
                return WebContentFormat.Json;
            }
        }
    }
}