using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace DAM.Backend.Services.Formatters;

// public class FileOutputFormatter : OutputFormatter
// {
//     public FileOutputFormatter()
//     {
//         SupportedMediaTypes.Add("image/png");
//         SupportedMediaTypes.Add("image/webp");
//         SupportedMediaTypes.Add("image/gif");
//         SupportedMediaTypes.Add("image/jpg");
//         SupportedMediaTypes.Add("image/jpeg");
//         //Do we need all of these?
//         //todo: what types do we want to support?
//     }
//
//
//     public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
//     {
//         if (context.Object == null)
//         {
//             throw new NotImplementedException();
//         }
//
//         if (context.Object is byte[] == false)
//         {
//             throw new NotImplementedException();
//         }
//
//         await context.HttpContext.Response.Body.WriteAsync((byte[])context.Object);
//     }
// }

public class FileOutputFormatter : OutputFormatter
{
    public FileOutputFormatter()
    {
        SupportedMediaTypes.Add("image/png");
        SupportedMediaTypes.Add("image/webp");
        SupportedMediaTypes.Add("image/gif");
        SupportedMediaTypes.Add("image/jpg");
        SupportedMediaTypes.Add("image/jpeg");
    }

    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        // Only handle FileContentResult objects
        return context.Object is FileContentResult;
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        var response = context.HttpContext.Response;

        if (context.Object is FileContentResult fileResult)
        {
            response.ContentType = fileResult.ContentType;
            
            if (fileResult.FileDownloadName != null)
            {
                response.Headers.ContentDisposition = $"attachment; filename={fileResult.FileDownloadName}";
            }

            await response.Body.WriteAsync(fileResult.FileContents, 0, fileResult.FileContents.Length);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported object type: {context.ObjectType}");
        }
    }
}