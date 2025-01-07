namespace API_Demo_Authen_Author.Services
{
    public interface IEmailService
    {
        bool SendEmail(string to, string subject, string body);

        bool ReSendToken(string email, int userId);
    }
}
