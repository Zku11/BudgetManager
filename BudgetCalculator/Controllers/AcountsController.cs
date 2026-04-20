using AutoMapper;
using BudgetCalculator.Models;
using BudgetCalculator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetCalculator.Controllers
{
    public class AcountsController : Controller
    {
        private readonly IAcountTypesRepository acountTypesRepository;
        private readonly IUserServices userServices;
        private readonly IAcountsRepository acountsRepository;
        private readonly IMapper mapper;
        private readonly ITransactionsRepository transactionsRepository;
        private readonly IReportsService reportsService;

        public AcountsController(IAcountTypesRepository acountTypesRepository,
            IUserServices userServices,
            IAcountsRepository acountsRepository,
            IMapper mapper,
            ITransactionsRepository transactionsRepository,
            IReportsService reportsService) 
        {
            this.acountTypesRepository = acountTypesRepository;
            this.userServices = userServices;
            this.acountsRepository = acountsRepository;
            this.mapper = mapper;
            this.transactionsRepository = transactionsRepository;
            this.reportsService = reportsService;
        }

        public async Task<IActionResult> Index()
        {
            int UserId = userServices.GetUserId();
            var acountsWithType = await acountsRepository.Search(UserId);
            var model = acountsWithType.GroupBy(x => x.AcountType).Select(x => new AcountsIndexViewModel {
                AcountType = x.Key,
                Acounts = x.AsEnumerable()
            }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Detail(int id, int month, int year)
        {
            int userId = userServices.GetUserId();
            var acount = await acountsRepository.GetById(id, userId);
            if (acount is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            ViewBag.Acount = acount.Name;

            var model = await reportsService.GetTransactionReportByAcount(userId, acount.Id, month, year, ViewBag);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAcount()
        {
            int userId = userServices.GetUserId();
            CreateAcountViewModel model = new CreateAcountViewModel();
            model.AcountTypes = await ObtainAcountTypes(userId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAcount(CreateAcountViewModel createAcountViewModel)
        {
            int userId = userServices.GetUserId();
            var acountType = await acountTypesRepository.GetById(createAcountViewModel.AcountTypeId, userId);
            if (acountType is null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            if (!ModelState.IsValid)
            {
                //createAcountViewModel.AcountTypes = await ObtainAcountTypes(userId);
                //return View(createAcountViewModel);
            }

            await acountsRepository.Create(createAcountViewModel);
            return View("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtainAcountTypes(int userId)
        {
            IEnumerable<AcountType> acountTypes = await acountTypesRepository.GetByUser(userId);
            return acountTypes.Select(x => new SelectListItem(x.Name, x.Id.ToString()));
        }

        public async Task<IActionResult> Edit(int Id)
        {
            int userId = userServices.GetUserId();
            Acount acount = await acountsRepository.GetById(Id, userId);

            if(acount is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            var model = mapper.Map<CreateAcountViewModel>(acount);

            model.AcountTypes = await ObtainAcountTypes(userId);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateAcountViewModel createAcountViewModel)
        {
            int userId = userServices.GetUserId();
            var acount = await acountsRepository.GetById(createAcountViewModel.Id, userId);

            if (acount is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            var acountType = await acountTypesRepository.GetById(createAcountViewModel.AcountTypeId, userId);

            if (acountType is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await acountsRepository.Update(createAcountViewModel);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = userServices.GetUserId();
            var acount = await acountsRepository.GetById(id, userId);
            if (acount is null) {
                return RedirectToAction("NotFound", "Home");
            }
            return View(acount);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAcount(int id)
        {
            int userId = userServices.GetUserId();
            var acount = await acountsRepository.GetById(id, userId);
            if (acount is null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            await acountsRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
