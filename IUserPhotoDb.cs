namespace image_db
{
    public interface IUserPhotoDb
    {
        void Put(string accountSid, byte[] imageBytes);
        byte[] Get(string accountSid);
    }
}