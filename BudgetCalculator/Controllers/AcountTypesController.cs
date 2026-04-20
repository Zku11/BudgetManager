using BudgetCalculator.Models;
using BudgetCalculator.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace BudgetCalculator.Controllers
{
    public class AcountTypesController : Controller
    {
        private readonly IAcountTypesRepository acountTypesRepository;
        private readonly IUserServices userServices;

        public AcountTypesController(IAcountTypesRepository acountTypesRepository, IUserServices userServices) {
            this.acountTypesRepository = acountTypesRepository;
            this.userServices = userServices;
        }

        public async Task<IActionResult> Index()
        {
            int userId = userServices.GetUserId();
            var acountTypes = await acountTypesRepository.GetByUser(userId);
            return View(acountTypes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AcountType acountType)
        {
            if (!ModelState.IsValid)
            {
                return View(acountType);
            }
            int userId = userServices.GetUserId();
            acountType.UserId = userId;
            bool alreadyExists = await acountTypesRepository.AlreadyExists(acountType.Name, acountType.UserId);
            if (alreadyExists) {
                ModelState.AddModelError(nameof(acountType.Name), $"El nombre de cuenta {acountType.Name} ya existe");
                return View(acountType);
            }
            acountTypesRepository.Create(acountType);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> AcountTypeLiveVerify(string Name)
        {
            int userId = userServices.GetUserId();
            bool alreadyExists = await acountTypesRepository.AlreadyExists(Name, userId);
            if (alreadyExists) {
                return Json($"El nombre {Name} ya existe en la base de datos");
            }
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int userId = userServices.GetUserId();
            AcountType acountType = await acountTypesRepository.GetById(id, userId);
            if (acountType == null) {
                return RedirectToAction("NotFound", "Home");
            }
            return View(acountType);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AcountType acountType)
        {
            int userId = userServices.GetUserId();
            var existsAcountType = await acountTypesRepository.GetById(acountType.Id, userId);
            if (existsAcountType == null) {
                return RedirectToAction("NotFound", "Home");
            }
            await acountTypesRepository.Update(acountType);
            return RedirectToAction("Index");
        }

        public IActionResult NotFound()
        {
            return View();
        }

        public async Task<IActionResult> Delete(int id)
        {
            int userId = userServices.GetUserId();
            AcountType acountType = await acountTypesRepository.GetById(id, userId);
            if (acountType == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            return View(acountType);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAcountType(int id)
        {
            int userId = userServices.GetUserId();
            AcountType acountType = await acountTypesRepository.GetById(id, userId);
            if (acountType == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            await acountTypesRepository.Delete(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> OrderTypes([FromBody] int[] ids)
        {
            int userId = userServices.GetUserId();
            IEnumerable<AcountType> acountTypes = await acountTypesRepository.GetByUser(userId);
            IEnumerable<int> acounTypesIds = acountTypes.Select(x => x.Id);
            var differentIds = ids.Except(acounTypesIds).ToList();
            if (differentIds.Any()) {
                return Forbid();
            }
            var orderedAcountTypes = ids.Select((value, index) => new AcountType() {Id = value, ShowOrder = index}).AsEnumerable();
            await acountTypesRepository.Order(orderedAcountTypes);
            return Ok();
        }
    }
}
