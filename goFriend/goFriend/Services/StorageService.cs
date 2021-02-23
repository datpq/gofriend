using System;
using System.Diagnostics;
using System.IO;
using Azure.Storage.Blobs;
using goFriend.DataModel;

namespace goFriend.Services
{
    public class StorageService : IStorageService
    {
        private readonly ILogger _logger;
        private readonly IMediaService _mediaService;
        private const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=gofriendstorage;AccountKey=8aUIkXRCsKD/uy3rA5aa1kzt4rtjCz21UNV1X7vbjn83GKeAj2HQb92lUqu7fvCkIxRlcjCyT/lGihUmctb44w==;EndpointSuffix=core.windows.net";
        private const string StorageKey = "8aUIkXRCsKD/uy3rA5aa1kzt4rtjCz21UNV1X7vbjn83GKeAj2HQb92lUqu7fvCkIxRlcjCyT/lGihUmctb44w==";

        public StorageService(ILogger logger, IMediaService mediaService = null)
        {
            _logger = logger;
            _mediaService = mediaService;
        }

        public bool Upload(StorageContainer container, string localFilePath, string remoteFilePath, float width = 0, float height = 0)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                _logger.Debug($"Upload.BEGIN(container={container}, localFilePath={localFilePath}, remoteFilePath={remoteFilePath}, width={width}, height={height})");
                var containerClient = new BlobContainerClient(StorageConnectionString, container.ToString());
                //containerClient.Create();
                var blobClient = containerClient.GetBlobClient(remoteFilePath);
                var localFileToUpload = localFilePath;
                if (width != 0 && height != 0)
                {
                    _logger.Debug("Resizing image before sending.");
                    localFileToUpload = Path.GetTempFileName();
                    File.WriteAllBytes(localFileToUpload, _mediaService.ResizeImage(File.ReadAllBytes(localFilePath), width, height));
                }
                _logger.Debug($"Deleting file if exists: {remoteFilePath}");
                blobClient.DeleteIfExists();
                _logger.Debug($"Uploading file...{localFileToUpload}");
                blobClient.Upload(localFileToUpload);
                if (width != 0 && height != 0)
                {
                    File.Delete(localFileToUpload);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                return false;
            }
            finally
            {
                _logger.Debug($"Upload.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }

        public bool Rename(StorageContainer container, string oldFilePath, string newFilePath)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                _logger.Debug($"Rename.BEGIN(container={container}, oldFilePath={oldFilePath}, newFilePath={newFilePath})");
                var containerClient = new BlobContainerClient(StorageConnectionString, container.ToString());
                //chatContainerClient.Create();
                var blobOldFile = containerClient.GetBlobClient(oldFilePath);
                var blobNewFile = containerClient.GetBlobClient(newFilePath);
                blobNewFile.StartCopyFromUri(blobOldFile.Uri);
                blobOldFile.DeleteIfExists();
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                return false;
            }
            finally
            {
                _logger.Debug($"Rename.END(ProcessingTime={stopWatch.Elapsed.ToStringStandardFormat()})");
            }
        }
    }
}
