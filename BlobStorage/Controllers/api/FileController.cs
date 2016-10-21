using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BlobStorage.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Net.Http.Headers;

namespace BlobStorage.Controllers.api
{
    public class FileController : ApiController
    {
        CloudStorageAccount _storageAccount =
           CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

        [HttpGet]
        [Route("~/api/file/getallfiles")]
        public async Task<HttpResponseMessage> GetBlobs()
        {
            try
            {
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");

                var blobList = container.ListBlobs(useFlatBlobListing: true);
                string blobPrefix = null;
                bool useFlatBlobListing = false;
                var listOfFileNames = new List<string>();
                var blobs = container.ListBlobs(blobPrefix, useFlatBlobListing, BlobListingDetails.None);
                var folders = blobs.Where(b => b as CloudBlobDirectory != null).ToList();
                foreach (var folder in folders)
                {
                    listOfFileNames.Add(folder.Uri.Segments.Last().Remove(folder.Uri.Segments.Last().Length - 1));
                }
                useFlatBlobListing = true;
                foreach (var blob in blobList)
                {
                    var blobFileName = blob.Uri.Segments.Last();
                    listOfFileNames.Add(blobFileName);
                }
                return Request.CreateResponse(HttpStatusCode.OK, listOfFileNames);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        //[HttpGet]
        //[Route("~/api/file/{folderGuid}/{fileName}")]
        //public async Task<HttpResponseMessage> GetBlobFile(string folderGuid, string fileName)
        //{
        //    try
        //    {
        //        Guid newGuid;
        //        if (!Guid.TryParse(folderGuid, out newGuid))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid folder name : " + folderGuid);
        //        }
        //        var localFolderToStore = ConfigurationManager.AppSettings["LocalStorage"];
        //        //var localFolderToStore = System.IO.Directory.CreateDirectory("BlobFolder");
        //        //string folderName = @"c:\BlobFolder";
        //        //string pathString = System.IO.Path.GetFullPath(folderName);
        //        //System.IO.Directory.CreateDirectory(pathString);
        //        CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
        //        CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");
        //        var blobs = container.ListBlobs(null, false, BlobListingDetails.None);
        //        var folders = blobs.Where(b => b as CloudBlobDirectory != null).ToList();
        //        var listOfFileNames = new List<string>();
        //        foreach (var folder in folders)
        //        {
        //            listOfFileNames.Add(folder.Uri.Segments.Last().Remove(folder.Uri.Segments.Last().Length - 1));
        //        }
        //        if (!listOfFileNames.Contains(folderGuid))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "Folder" + folderGuid + " not found!");
        //        }
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(folderGuid + "/" + fileName);
        //        if (!blockBlob.Exists())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "No such file exists!");
        //        }

        //        using (var fileStream = File.OpenWrite(Path.Combine(localFolderToStore.ToString(), fileName)))
        //        {
        //            blockBlob.DownloadToStream(fileStream);
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, "File downloaded to the location:" + localFolderToStore);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
        //    }
        //}

        [HttpDelete]
        [Route("~/api/file/{folderGuid}/{fileName}")]
        public async Task<HttpResponseMessage> DeleteBlobFile(string folderGuid, string fileName)
        {
            try
            {
                Guid newGuid;
                if (!Guid.TryParse(folderGuid, out newGuid))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid folder name : " + folderGuid);
                }
                var localFolderToStore = ConfigurationManager.AppSettings["LocalStorage"];
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");

                var blobs = container.ListBlobs(null, false, BlobListingDetails.None);
                var folders = blobs.Where(b => b as CloudBlobDirectory != null).ToList();
                var listOfFileNames = new List<string>();
                foreach (var folder in folders)
                {
                    listOfFileNames.Add(folder.Uri.Segments.Last().Remove(folder.Uri.Segments.Last().Length - 1));
                }
                if (!listOfFileNames.Contains(folderGuid))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Folder" + folderGuid + " not found!");
                }
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(folderGuid + "/" + fileName);
                if (!blockBlob.Exists())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No such file exists!");
                }
                blockBlob.Delete();
                return Request.CreateResponse(HttpStatusCode.OK, "File "+ fileName + " Deleted successfully!");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        //[HttpPost]
        //[Route("~/api/file/Upload/{folderGuid}")]
        //public async Task<HttpResponseMessage> Upload(string folderGuid) //Accept the posted file  
        //{
        //    Guid newGuid;
        //    if (!Guid.TryParse(folderGuid, out newGuid))
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid folder name : " + folderGuid);
        //    }
        //    var localFolderToStore = ConfigurationManager.AppSettings["LocalStorage"];
        //    CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
        //    CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");
        //    container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

        //    var blobs = container.ListBlobs(null, false, BlobListingDetails.None);
        //    var folders = blobs.Where(b => b as CloudBlobDirectory != null).ToList();
        //    var listOfFileNames = new List<string>();
        //    foreach (var folder in folders)
        //    {
        //        listOfFileNames.Add(folder.Uri.Segments.Last().Remove(folder.Uri.Segments.Last().Length - 1));
        //    }
        //    if (!listOfFileNames.Contains(folderGuid))
        //    {
        //        return Request.CreateResponse(HttpStatusCode.NotFound, "Folder" + folderGuid + " not found!");
        //    }
        //    var uploadPath = HttpContext.Current.Server.MapPath("~/Uploads");

        //    var multipartFormDataStreamProvider = new UploadMultipartFormProvider(uploadPath);

        //    // Read the MIME multipart asynchronously 
        //    await Request.Content.ReadAsMultipartAsync(multipartFormDataStreamProvider);

        //    var localFilePath = multipartFormDataStreamProvider
        //        .FileData.Select(multiPartData => multiPartData.LocalFileName).FirstOrDefault();
        //    var FileName = Path.GetFileName(localFilePath);
        //    CloudBlockBlob blockBlob = container.GetBlockBlobReference(folderGuid + "/" + FileName);
        //    using (var fileStream = System.IO.File.OpenRead(localFilePath))
        //    {
        //        blockBlob.UploadFromStream(fileStream);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, "File Uploaded successfully."); ;
        //}
        //[HttpGet]
        //[Route("~/api/file/{folderGuid}/{fileName}")]
        //public async Task<HttpResponseMessage> GetBlobFile(string folderGuid, string fileName)
        //{
        //    try
        //    {
        //        Guid newGuid;
        //        if (!Guid.TryParse(folderGuid, out newGuid))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid folder name : " + folderGuid);
        //        }
        //        CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
        //        CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");
        //        var blobs = container.ListBlobs(null, false, BlobListingDetails.None);
        //        var folders = blobs.Where(b => b as CloudBlobDirectory != null).ToList();
        //        var listOfFileNames = new List<string>();
        //        foreach (var folder in folders)
        //        {
        //            listOfFileNames.Add(folder.Uri.Segments.Last().Remove(folder.Uri.Segments.Last().Length - 1));
        //        }
        //        if (!listOfFileNames.Contains(folderGuid))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "Folder" + folderGuid + " not found!");
        //        }
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(folderGuid + "/" + fileName);
        //        if (!blockBlob.Exists())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "No such file exists!");
        //        }

        //        var memoryStream = new MemoryStream();
        //        blockBlob.DownloadToStream(memoryStream);
        //        return Request.CreateResponse(HttpStatusCode.OK, "File downloaded to the location:" + localFolderToStore);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
        //    }
        //}

        [HttpGet]
        [Route("~/api/file/{folderGuid}/{fileName}")]
        public async Task<HttpResponseMessage> DownloadBlob(string folderGuid, string fileName)
        {
            Guid newGuid;
            if (!Guid.TryParse(folderGuid, out newGuid))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid folder name : " + folderGuid);
            }
            var localFolderToStore = ConfigurationManager.AppSettings["LocalStorage"];
            
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");
            var blobs = container.ListBlobs(null, false, BlobListingDetails.None);
            var folders = blobs.Where(b => b as CloudBlobDirectory != null).ToList();
            var listOfFileNames = new List<string>();
            foreach (var folder in folders)
            {
                listOfFileNames.Add(folder.Uri.Segments.Last().Remove(folder.Uri.Segments.Last().Length - 1));
            }
            if (!listOfFileNames.Contains(folderGuid))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Folder" + folderGuid + " not found!");
            }
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(folderGuid + "/" + fileName);
            if (!blockBlob.Exists())
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "No such file exists!");
            }
            else
            {
                var blob = blockBlob;
                var ms = new MemoryStream();
                await blob.DownloadToStreamAsync(ms);

                var lastPos = blob.Name.LastIndexOf('/');
                var filename = blob.Name.Substring(lastPos + 1, blob.Name.Length - lastPos - 1);

                var download = new MimeMultipart.BlobDownloadModel
                {
                    BlobStream = ms,
                    BlobFileName = filename,
                    BlobLength = blob.Properties.Length,
                    BlobContentType = blob.Properties.ContentType
                };
                if (download == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No such file exists!");
                }

                // Resetting the stream position; because download was not working
                download.BlobStream.Position = 0;

                // Create response message with blob stream as its content
                var message = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(download.BlobStream)
                };

                // Set content headers
                message.Content.Headers.ContentLength = download.BlobLength;
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(download.BlobContentType);
                message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = HttpUtility.UrlDecode(download.BlobFileName),
                    Size = download.BlobLength
                };
                return message;
            }
        }
        public async Task<List<MimeMultipart.BlobUploadModel>> UploadBlobs(HttpContent httpContent, Guid folderGuid)
        {
            var blobUploadProvider = new BlobStorageUploadProvider();
            blobUploadProvider.folderGuid = folderGuid;
            var list = await httpContent.ReadAsMultipartAsync(blobUploadProvider)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        throw task.Exception;
                    }

                    var provider = task.Result;
                    return provider.Uploads.ToList();
                });

            return list;
        }

        [HttpPost]
        [Route("~/api/file/Upload/{folderGuid}")]
        public async Task<HttpResponseMessage> Upload(string folderGuid) //Accept the posted file  
        {
            try
            {
                Guid newGuid;
                if (!Guid.TryParse(folderGuid, out newGuid))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid folder name : " + folderGuid);
                }
                // This endpoint only supports multipart form data
                if (!Request.Content.IsMimeMultipartContent("form-data"))
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                }

                var result = await UploadBlobs(Request.Content, newGuid);
                if (result != null && result.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,ex.Message.ToString());
            }
        }
    }

    public class BlobStorageUploadProvider : MultipartFileStreamProvider
    {
        public List<MimeMultipart.BlobUploadModel> Uploads { get; set; }
        CloudStorageAccount _storageAccount =
          CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        public BlobStorageUploadProvider() : base(Path.GetTempPath())
        {
            Uploads = new List<MimeMultipart.BlobUploadModel>();
        }
        public Guid folderGuid;
        public override Task ExecutePostProcessingAsync()
        {
            foreach (var fileData in FileData)
            {
                var fileName = Path.GetFileName(fileData.Headers.ContentDisposition.FileName.Trim('"'));

                // Retrieve reference to a blob
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");
                var blobContainer = container;
                var blob = blobContainer.GetBlockBlobReference(folderGuid + "/" + fileName);

                // Setting the blob content type
                blob.Properties.ContentType = fileData.Headers.ContentType.MediaType;
                using (var fs = File.OpenRead(fileData.LocalFileName))
                {
                    blob.UploadFromStream(fs);
                }

                // Deleteting local file from disk
                File.Delete(fileData.LocalFileName);

                // Creating blob upload model with properties from blob info
                var blobUpload = new MimeMultipart.BlobUploadModel
                {
                    FileName = blob.Name,
                    FileUrl = blob.Uri.AbsoluteUri,
                    FileSizeInBytes = blob.Properties.Length
                };

                // Adding uploaded blob to the list
                Uploads.Add(blobUpload);
            }

            return base.ExecutePostProcessingAsync();
        }
    }
}
