using System.Threading.Tasks;

namespace TicTacToe.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmail(string emailTo, string subject, string message);
    }
}