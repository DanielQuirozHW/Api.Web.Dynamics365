using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;

namespace Api.Web.Dynamics365.Servicios
{
    public interface IErrorLogService
    {
        Task CreateErrorLogAsync(ErrorLog errorLogDto);
    }
    public class ErrorLogService : IErrorLogService
    {
        private readonly ApplicationDbContext _context;

        public ErrorLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateErrorLogAsync(ErrorLog errorLogDto)
        {
            var errorLog = new ErrorLog
            {
                Timestamp = errorLogDto.Timestamp,
                Level = errorLogDto.Level,
                Message = errorLogDto.Message,
                ExceptionDetails = errorLogDto.ExceptionDetails,
                Source = errorLogDto.Source,
                Url = errorLogDto.Url,
                UserId = errorLogDto.UserId,
                IPAddress = errorLogDto.IPAddress,
                StackTrace = errorLogDto.StackTrace
            };

            _context.ErrorLogs.Add(errorLog);
            await _context.SaveChangesAsync();
        }
    }
}
