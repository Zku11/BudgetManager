using AutoMapper;
using BudgetCalculator.Models;
using BudgetCalculator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.Collections;
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

        public IActionResult Weekly()
        {
            return View();
        }

        public IActionResult Monthly()
        {
            return View();
        }

        public IActionResult ExcelReport()
        {
            return View();
        }

        public IActionResult Calendar()
        {
            return View();
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
