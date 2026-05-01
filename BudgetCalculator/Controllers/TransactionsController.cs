using AutoMapper;
using BudgetCalculator.Models;
using BudgetCalculator.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace BudgetCalculator.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IUserServices userServices;
        private readonly ITransactionsRepository transactionsRepository;
        private readonly IAcountsRepository acountsRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IMapper mapper;
        private readonly IReportsService reportsService;

        public TransactionsController(IUserServices userServices,
            ITransactionsRepository transactionsRepository,
            IAcountsRepository acountsRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            IReportsService reportsService)
        {
            this.userServices = userServices;
            this.transactionsRepository = transactionsRepository;
            this.acountsRepository = acountsRepository;
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
            this.reportsService = reportsService;
        }

        public async Task<IActionResult> Index(int month, int year)
        {
            int userId = userServices.GetUserId();
            var model = await reportsService.GetTransactionReport(userId, month, year, ViewBag);
            return View(model);
        }

        public async Task<IActionResult> Weekly(int month, int year)
        {
            int userId = userServices.GetUserId();
            IEnumerable<WeeklyReport> model = await reportsService.GetByWeeklyReport(userId, month, year, ViewBag);
            var grouped = model.GroupBy(x => x.Weeks).Select(x => new WeeklyReport()
            {
                Weeks = x.Key,
                Incomes = x.Where(x => x.OperationTypeId == OperationType.Income).Select(x => x.Monto).FirstOrDefault(),
                Expenses = x.Where(x => x.OperationTypeId == OperationType.Expense).Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            if (month == 0 || year == 0)
            {
                var today = DateTime.Today;
                month = today.Month;
                year = today.Year;
            }
            var referenceDateTime = new DateTime(year, month, 1);
            var monthDays = Enumerable.Range(1, referenceDateTime.AddMonths(1).AddDays(-1).Day);
            var segmentedDays = monthDays.Chunk(7).ToList();
            for (int i = 0; i < segmentedDays.Count; i++)
            {
                var week = i + 1;
                var startDate = new DateTime(year, month, segmentedDays[i].First());
                var endDate = new DateTime(year, month, segmentedDays[i].Last());
                var weekGroup = grouped.FirstOrDefault(x => x.Weeks == week);
                if (weekGroup == null)
                {
                    grouped.Add(
                        new WeeklyReport()
                        {
                            Weeks = week,
                            StartDate = startDate,
                            EndDate = endDate
                        }
                        );
                }
                else
                {
                    weekGroup.StartDate = startDate;
                    weekGroup.EndDate = endDate;
                }
            }
            grouped = grouped.OrderByDescending(x => x.Weeks).ToList();
            var viewModel = new WeeklyReportViewModel()
            {
                WeeklyReports = grouped,
                ReferenceDate = referenceDateTime
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Monthly(int year)
        {
            int userId = userServices.GetUserId();
            if (year == 0)
            {
                year = DateTime.Today.Year;
            }
            var transactionsByMonth = await transactionsRepository.GetByMonth(userId, year);
            var groupedtransactionsByMonth = transactionsByMonth.GroupBy(x => x.Month).Select(
                x => new MonthlyReport()
                {
                    Month = x.Key,
                    Incomes = x.Where(x => x.OperationTypeId == OperationType.Income).Select(x => x.Monto).FirstOrDefault(),
                    Expenses = x.Where(x => x.OperationTypeId == OperationType.Expense).Select(x => x.Monto).FirstOrDefault()
                }
                ).ToList();
            for (int month = 1; month <= 12; month++)
            {
                var transaction = groupedtransactionsByMonth.FirstOrDefault(x => x.Month == month);
                var referenceDate = new DateTime(year, month, 1);
                if (transaction is null)
                {
                    groupedtransactionsByMonth.Add(
                        new MonthlyReport()
                        {
                            Month = month,
                            ReferenceDate = referenceDate
                        }
                        );
                }
                else
                {
                    transaction.ReferenceDate = referenceDate;
                }
                groupedtransactionsByMonth = groupedtransactionsByMonth.OrderByDescending(x => x.Month).ToList();
            }
            var viewModel = new MonthlyReportViewModel()
            {
                Year = year,
                TransactionsByMonth= groupedtransactionsByMonth,
            };
            return View(viewModel);
        }

        public IActionResult ExcelReport()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExcelExportByMonth(int month, int year)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            int userId = userServices.GetUserId();
            IEnumerable<Transaction> transactions = await transactionsRepository.GetByUserId(new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            });
            string fileName = $"Manejo presupuesto - {startDate.ToString("MMM yyy")}.xlsx";
            return GenerateExcel(fileName, transactions);
        }

        [HttpGet]
        public async Task<FileResult> ExcelExportByYear(int year)
        {
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = startDate.AddYears(1).AddDays(-1);
            int userId = userServices.GetUserId();
            IEnumerable<Transaction> transactions = await transactionsRepository.GetByUserId(new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            });
            string fileName = $"Manejo Presupuesto - {startDate.ToString("yyyy")}.xlsx";
            return GenerateExcel(fileName, transactions);
        }

        [HttpGet]
        public async Task<FileResult> ExcelExportAll()
        {
            DateTime startDate = DateTime.Today.AddYears(-100);
            DateTime endDate = startDate.AddYears(100);
            int userId = userServices.GetUserId();
            IEnumerable<Transaction> transactions = await transactionsRepository.GetByUserId(new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            });
            string fileName = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";
            return GenerateExcel(fileName, transactions);
        }

        private FileResult GenerateExcel(string fileName, IEnumerable<Transaction> transactions)
        {
            DataTable dataTable = new DataTable("Transactions");
            dataTable.Columns.AddRange(new DataColumn[] {
                new DataColumn("Date"),
                new DataColumn("Acount"),
                new DataColumn("Category"),
                new DataColumn("Note"),
                new DataColumn("Monto"),
                new DataColumn("Income/Expense")
            });

            foreach (Transaction transaction in transactions) { 
                dataTable.Rows.Add(
                    transaction.TransactionDate,
                    transaction.Acount,
                    transaction.Category,
                    transaction.Nota,
                    transaction.Monto,
                    transaction.OperationTypeId
                    );
            }

            using (XLWorkbook wbWorkbook = new XLWorkbook())
            {
                wbWorkbook.Worksheets.Add(dataTable);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wbWorkbook.SaveAs( memoryStream );
                    return File(
                        memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheet.sheet",
                        fileName);
                }
            }
            
        }

        public IActionResult Calendar()
        {
            return View();
        }

        public async Task<JsonResult> CalendarReport()
        {
            DateTime startDate = DateTime.Today.AddYears(-100);
            DateTime endDate = startDate.AddYears(100);
            int userId = userServices.GetUserId();
            IEnumerable<Transaction> transactions = await transactionsRepository.GetByUserId(new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            });
            var calendarEvents = transactions.Select(x => new CalendarEvent()
            {
                Title = x.Monto.ToString("N"),
                start = x.TransactionDate.ToString("yyyy-MM-d"),
                end = x.TransactionDate.ToString("yyyy-MM-d"),
                Color = x.OperationTypeId == OperationType.Expense ? "Red" : null
            });
            return Json(calendarEvents);
        }

        public async Task<JsonResult> GetTransactionsByDate(DateTime date)
        {
            int userId = userServices.GetUserId();
            IEnumerable<Transaction> transactions = await transactionsRepository.GetByUserId(new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = date,
                EndDate = date
            });
            return Json(transactions);
        }

        public async Task<IActionResult> Create()
        {
            int userId = userServices.GetUserId();
            var model = new TransactionCreationViewModel();
            model.Acounts = await GetAcounts(userId);
            model.OperationTypeId = OperationType.Income;
            model.Categories = await GetCategories(userId, model.OperationTypeId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TransactionCreationViewModel transactionCreationViewModel)
        {
            int userId = userServices.GetUserId();
            if (!ModelState.IsValid)
            {
                transactionCreationViewModel.Acounts = await GetAcounts(userId);
                transactionCreationViewModel.Categories = await GetCategories(userId, transactionCreationViewModel.OperationTypeId);
                //return View(transactionCreationViewModel);
            }
            var acount = await acountsRepository.GetById(transactionCreationViewModel.AcountId, userId);
            if (acount is null)
            {
                RedirectToAction("NotFound", "Home");
            }
            var category = await categoryRepository.GetById(transactionCreationViewModel.CategoryId, userId);
            if (category is null)
            {
                RedirectToAction("NotFound", "Home");
            }
            transactionCreationViewModel.UserId = userId;
            if (transactionCreationViewModel.OperationTypeId == OperationType.Expense)
            {
                transactionCreationViewModel.Monto *= -1;
            }
            await transactionsRepository.Create(transactionCreationViewModel);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> GetAcounts(int userId)
        {
            var acounts = await acountsRepository.Search(userId);
            return acounts.Select(x => new SelectListItem(x.Name, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> GetCategories(int userId, OperationType operationType)
        {
            var categories = await categoryRepository.Get(userId, operationType);
            return categories.Select(x => new SelectListItem(x.Name, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> GetCategories([FromBody] OperationType operationType)
        {
            int userId = userServices.GetUserId();
            var categories = await GetCategories(userId, operationType);
            return Ok(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string returnUrl)
        {
            int userId = userServices.GetUserId();
            var transaction = await transactionsRepository.GetById(id, userId);
            if (transaction == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var model = mapper.Map<UpdateTransactionViewModel>(transaction);
            model.PreviousMonto = model.Monto;
            if(model.OperationTypeId == OperationType.Expense)
            {
                model.PreviousMonto = model.Monto * -1;
            }
            model.PreviousAcountId = transaction.AcountId;
            model.Categories = await GetCategories(userId, transaction.OperationTypeId);
            model.Acounts = await GetAcounts(userId);
            model.returnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateTransactionViewModel viewModel)
        {
            int userId = userServices.GetUserId();
            if (!ModelState.IsValid)
            {
                viewModel.Acounts = await GetAcounts(userId);
                viewModel.Categories = await GetCategories(userId, viewModel.OperationTypeId);
                //return View(viewModel);
            }
            var acount = await acountsRepository.GetById(viewModel.AcountId, userId);
            if (acount is null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var category = await categoryRepository.GetById(viewModel.CategoryId, userId);
            if (category is null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var transaction = mapper.Map<Transaction>(viewModel);
            if (viewModel.OperationTypeId == OperationType.Expense)
            {
                transaction.Monto *= -1; 
            }
            await transactionsRepository.Update(transaction, viewModel.PreviousMonto, viewModel.PreviousAcountId);
            if (string.IsNullOrEmpty(viewModel.returnUrl))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(viewModel.returnUrl);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string returnUrl)
        {
            int userId = userServices.GetUserId();
            var transaction = await transactionsRepository.GetById(id, userId);
            if (transaction is null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            await transactionsRepository.Delete(id);
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(returnUrl);
            }
        }
    }
}
