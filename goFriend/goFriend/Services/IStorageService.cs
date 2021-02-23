namespace goFriend.Services
{
    public enum StorageContainer
    {
        chat,
        logs
    }

    public interface IStorageService
    {
        bool Upload(StorageContainer container, string localFilePath, string remoteFilePath, float width = 0, float height = 0);
        bool Rename(StorageContainer container, string oldFilePath, string newFilePath);
    }
}
