﻿using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Tracing.Common
{
    public class MessagePackOutputFormatter : IOutputFormatter //, IApiResponseTypeMetadataProvider
    {
        const string ContentType = "application/x-msgpack";
        static readonly string[] SupportedContentTypes = new[] { ContentType };

        readonly IFormatterResolver resolver;

        public MessagePackOutputFormatter()
            : this(null)
        {

        }
        public MessagePackOutputFormatter(IFormatterResolver resolver)
        {
            this.resolver = resolver ?? MessagePackSerializer.DefaultResolver;
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return ContentType == context.HttpContext.Request.ContentType;
        }

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            context.HttpContext.Response.ContentType = ContentType;

            // 'object' want to use anonymous type serialize, etc...
            if (context.ObjectType == typeof(object))
            {
                if (context.Object == null)
                {
                    context.HttpContext.Response.Body.WriteByte(MessagePackCode.Nil);
                    return Task.CompletedTask;
                }
                else
                {
                    // use concrete type.
                    MessagePackSerializer.NonGeneric.Serialize(context.Object.GetType(), context.HttpContext.Response.Body, context.Object, resolver);
                    return Task.CompletedTask;
                }
            }
            else
            {
                MessagePackSerializer.NonGeneric.Serialize(context.ObjectType, context.HttpContext.Response.Body, context.Object, resolver);
                return Task.CompletedTask;
            }
        }
    }

    public class MessagePackInputFormatter : IInputFormatter // , IApiRequestFormatMetadataProvider
    {
        const string ContentType = "application/x-msgpack";
        static readonly string[] SupportedContentTypes = new[] { ContentType };

        readonly IFormatterResolver resolver;

        public MessagePackInputFormatter()
            : this(null)
        {

        }

        public MessagePackInputFormatter(IFormatterResolver resolver)
        {
            this.resolver = resolver ?? MessagePackSerializer.DefaultResolver;
        }

        public bool CanRead(InputFormatterContext context)
        {
            return ContentType == context.HttpContext.Request.ContentType;
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var result = MessagePackSerializer.NonGeneric.Deserialize(context.ModelType, request.Body, resolver);
            return InputFormatterResult.SuccessAsync(result);
        }
    }
}
