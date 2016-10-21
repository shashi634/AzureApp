using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BlobStorage.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Xml.Linq;

namespace BlobStorage.Controllers.api
{

    public class CatalogController : ApiController
    {
        CloudStorageAccount _storageAccount =
            CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

        [HttpGet]
        [Route("~/api/catalog/getCalatlog/{id}")]
        public Book GetBooks(string id)
        {
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("catelog.xml");
            var xml = blockBlob.DownloadText();
            var xDoc = XDocument.Parse(xml);

            var query = from t in xDoc.Descendants("book")
                let xElement = t.Element("id")
                where xElement != null && xElement.Value.ToLower() == id
                select new
                {
                    ID = t.Element("id").Value,
                    Title = t.Element("title").Value,
                    Price = t.Element("price").Value,
                    Author = t.Element("author").Value,
                    PublishDate = t.Element("publish_date").Value,
                    Gener = t.Element("genre").Value
                };
            if (!query.Any())
            {
                return null;
            }
            var myBook = new Book();
            foreach (var item in query)
            {
                myBook.Title = item.Title;
                myBook.Author = item.Author;
                myBook.Genre = item.Gener;
                myBook.Price = Convert.ToDecimal(item.Price);
                myBook.PublishDate = Convert.ToDateTime(item.PublishDate);
                myBook.Id = item.ID;

            }
            return myBook;
        }

        [HttpPut]
        [Route("~/api/catalog/updateCalatlog/{id}")]
        public async Task<HttpResponseMessage> UpdateBooks(string id, Book userBook)
        {
            try
            {

                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");

                CloudBlockBlob blockBlob = container.GetBlockBlobReference("catelog.xml");
                var xml = blockBlob.DownloadText();
                var xDoc = XDocument.Parse(xml);

                var query = from t in xDoc.Descendants("book")
                    let xElement = t.Element("id")
                    where xElement != null && xElement.Value.ToLower() == id
                    select t;
                foreach (var x in query)
                {
                    x.Element("author").Value = userBook.Author;
                    x.Element("title").Value = userBook.Title;
                    x.Element("genre").Value = userBook.Genre;
                    x.Element("price").Value = userBook.Price.ToString(CultureInfo.InvariantCulture);
                    x.Element("publish_date").Value = userBook.PublishDate.ToString(CultureInfo.InvariantCulture);
                }
                await Task.Run(() => blockBlob.UploadText(xDoc.ToString()));
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpDelete]
        [Route("~/api/catalog/deleteCalatlog/{id}")]
        public async Task<HttpResponseMessage> DeleteBooks(string id)
        {
            try
            {
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("hckblobcontainer");

                CloudBlockBlob blockBlob = container.GetBlockBlobReference("catelog.xml");
                var xml = blockBlob.DownloadText();
                var xDoc = XDocument.Parse(xml);

                var query = from t in xDoc.Descendants("book")
                    let xElement = t.Element("id")
                    where xElement != null && xElement.Value.ToLower() == id
                    select t;
                query.ToList().ForEach(x => x.Remove());
                await Task.Run(() => blockBlob.UploadText(xDoc.ToString()));
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }
    }
}
