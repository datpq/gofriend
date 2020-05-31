namespace goFriend.Services
{
    public interface IStorageService
    {
        bool Upload(string localFilePath, string remoteFilePath);
        bool Rename(string oldFilePath, string newFilePath);
    }
}
