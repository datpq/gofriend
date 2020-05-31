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
        private const string ChatStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=gofriendstorage;AccountKey=8aUIkXRCsKD/uy3rA5aa1kzt4rtjCz21UNV1X7vbjn83GKeAj2HQb92lUqu7fvCkIxRlcjCyT/lGihUmctb44w==;EndpointSuffix=core.windows.net";
        private const string ChatStorageKey = "8aUIkXRCsKD/uy3rA5aa1kzt4rtjCz21UNV1X7vbjn83GKeAj2HQb92lUqu7fvCkIxRlcjCyT/lGihUmctb44w==";
        private const string ChatStorageContainer = "chat";

        public StorageService(ILogger logger, IMediaService mediaService = null)
        {
            _logger = logger;
            _mediaService = mediaService;
        }

        public bool Upload(string localFilePath, string remoteFilePath)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                _logger.Debug($"Upload.BEGIN(localFilePath={localFilePath}, remoteFilePath={remoteFilePath})");
                var chatContainerClient = new BlobContainerClient(ChatStorageConnectionString, ChatStorageContainer);
                //chatContainerClient.Create();
                var blobClient = chatContainerClient.GetBlobClient(remoteFilePath);
                var localSmallFilePath = Path.GetTempFileName();
                File.WriteAllBytes(localSmallFilePath, _mediaService.ResizeImage(File.ReadAllBytes(localFilePath), 640, 400));
                _logger.Debug($"localSmallFilePath={localSmallFilePath}");
                _logger.Debug($"Deleting file if exists: {remoteFilePath}");
                blobClient.DeleteIfExists();
                _logger.Debug("Uploading file...");
                blobClient.Upload(localSmallFilePath);
                File.Delete(localSmallFilePath);
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

        public bool Rename(string oldFilePath, string newFilePath)
        {
            var stopWatch = Stopwatch.StartNew();
            try
            {
                _logger.Debug($"Rename.BEGIN(oldFilePath={oldFilePath}, newFilePath={newFilePath})");
                var chatContainerClient = new BlobContainerClient(ChatStorageConnectionString, ChatStorageContainer);
                //chatContainerClient.Create();
                var blobOldFile = chatContainerClient.GetBlobClient(oldFilePath);
                var blobNewFile = chatContainerClient.GetBlobClient(newFilePath);
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
