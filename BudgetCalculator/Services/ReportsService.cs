using BudgetCalculator.Models;
using Microsoft.Identity.Client;

namespace BudgetCalculator.Services
{
    public interface IReportsService
    {
        Task<IEnumerable<WeeklyReport>> GetByWeeklyReport(int userId, int month, int year, dynamic ViewBag);
        Task<DetailedTransactionReport> GetTransactionReport(int userId, int month, int year, dynamic ViewBag);
        Task<DetailedTransactionReport> GetTransactionReportByAcount(int userId, int acountId, int month, int year, dynamic ViewBag);
    }

    public class ReportsService : IReportsService
    {
        private readonly ITransactionsRepository transactionsRepository;
        private readonly HttpContext httpContext;

        public ReportsService(ITransactionsRepository transactionsRepository, IHttpContextAccessor httpContextAccessor)
        {
            this.transactionsRepository = transactionsRepository;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<IEnumerable<WeeklyReport>> GetByWeeklyReport(int userId, int month, int year, dynamic ViewBag)
        {
            (DateTime startDate, DateTime endDate) = GenerateStartAndEndDates(month, year);
            var parameter = new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };
            AsignValuesToViewBag(ViewBag, startDate);
            var model = await transactionsRepository.GetByWeek(parameter);
            return model;
        }

        public async Task<DetailedTransactionReport> GetTransactionReport(int userId, int month, int year, dynamic ViewBag)
        {
            (DateTime startDate, DateTime endDate) = GenerateStartAndEndDates(month, year);
            var parameter = new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };
            var transactions = await transactionsRepository.GetByUserId(parameter);
            DetailedTransactionReport model = GenerateDetailedTransactionsReport(startDate, endDate, transactions);
            AsignValuesToViewBag(ViewBag, startDate);
            return model;
        }

        public async Task<DetailedTransactionReport> GetTransactionReportByAcount(int userId, int acountId, int month, int year, dynamic ViewBag)
        {
            (DateTime startDate, DateTime endDate) = GenerateStartAndEndDates(month, year);
            var getTransactionsByAcount = new GetTransactionsByAcount()
            {
                AcountId = acountId,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };
            var transactions = await transactionsRepository.GetByAcountId(getTransactionsByAcount);
            DetailedTransactionReport model = GenerateDetailedTransactionsReport(startDate, endDate, transactions);
            AsignValuesToViewBag(ViewBag, startDate);
            return model;
        }

        private void AsignValuesToViewBag(dynamic ViewBag, DateTime startDate)
        {
            ViewBag.previousMonth = startDate.AddMonths(-1).Month;
            ViewBag.previousYear = startDate.AddMonths(-1).Year;
            ViewBag.laterMonth = startDate.AddMonths(1).Month;
            ViewBag.laterYear = startDate.AddMonths(1).Year;
            ViewBag.returnUrl = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        private DetailedTransactionReport GenerateDetailedTransactionsReport(DateTime startDate, DateTime endDate, IEnumerable<Transaction> transactions)
        {
            var model = new DetailedTransactionReport();
            var transactionsByDate = transactions.OrderByDescending(x => x.TransactionDate).GroupBy(x => x.TransactionDate)
                .Select(group => new TransactionsByDate()
                {
                    TransactionDate = group.Key,
                    Transactions = group.AsEnumerable()
                });
            model.transactionsByDateList = transactionsByDate;
            model.StartDate = startDate;
            model.EndDate = endDate;
            return model;
        }

        private (DateTime startDate, DateTime endDate) GenerateStartAndEndDates(int month, int year)
        {
            DateTime startDate;
            DateTime endDate;
            if (month <= 0 || month > 12 || year <= 1900)
            {
                var today = DateTime.Today;
                startDate = new DateTime(today.Year, today.Month, 1);
            }
            else
            {
                startDate = new DateTime(year, month, 1);
            }
            endDate = startDate.AddMonths(1).AddDays(-1);
            return (startDate, endDate);
        }
    }
}
