namespace goFriend.Services
{
    public interface IStorageService
    {
        bool Upload(string localFilePath, string remoteFilePath, float width = 0, float height = 0);
        bool Rename(string oldFilePath, string newFilePath);
    }
}
