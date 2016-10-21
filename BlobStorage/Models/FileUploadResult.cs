using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace BlobStorage.Models
{
    public class FileUploadResult
    {
        public string LocalFilePath { get; set; }
        public string FileName { get; set; }
        public long FileLength { get; set; }
    }

    public class UploadMultipartFormProvider : MultipartFormDataStreamProvider
    {
        public UploadMultipartFormProvider(string rootPath) : base(rootPath) { }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            try
            {
                if (headers != null &&
                headers.ContentDisposition != null)
                {
                    return headers
                        .ContentDisposition
                        .FileName.TrimEnd('"').TrimStart('"');
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            

            return base.GetLocalFileName(headers);
        }
    }
    public class MimeMultipart : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                if (!actionContext.Request.Content.IsMimeMultipartContent())
                {

                    throw new HttpResponseException(
                        new HttpResponseMessage(
                            HttpStatusCode.UnsupportedMediaType)
                    );
                }
            }
            catch (Exception ex)
            {
                 
            }
            
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {

        }
        public class BlobDownloadModel
        {
            public MemoryStream BlobStream { get; set; }
            public string BlobFileName { get; set; }
            public string BlobContentType { get; set; }
            public long BlobLength { get; set; }
        }

        public class BlobUploadModel
        {
            public string FileName { get; set; }
            public string FileUrl { get; set; }
            public long FileSizeInBytes { get; set; }
            public long FileSizeInKb { get { return (long)Math.Ceiling((double)FileSizeInBytes / 1024); } }
        }
    }
}