namespace goFriend.Services
{
    public interface IMediaService
    {
        byte[] ResizeImage(byte[] imageData, float width, float height);
    }
}
