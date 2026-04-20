using BudgetCalculator.Models;
using BudgetCalculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetCalculator.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IUserServices userServices;

        public CategoryController(ICategoryRepository categoryRepository, IUserServices userServices)
        {
            this.categoryRepository = categoryRepository;
            this.userServices = userServices;
        }

        public async Task<IActionResult> Index()
        {
            int userId = userServices.GetUserId();
            var categories = await categoryRepository.GetByUserId(userId);
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            int userId = userServices.GetUserId();
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            category.UserId = userId;
            await categoryRepository.Create(category);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            int userId = userServices.GetUserId();
            var category = await categoryRepository.GetById(id, userId);
            if(category == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category categoryToEdit)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryToEdit);
            }
            int userId = userServices.GetUserId();
            var category = await categoryRepository.GetById(categoryToEdit.Id, userId);
            if (category == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            categoryToEdit.UserId = userId;
            await categoryRepository.Update(categoryToEdit);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            int userId = userServices.GetUserId();
            var category = await categoryRepository.GetById(id, userId);
            if (category == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            int userId = userServices.GetUserId();
            var category = await categoryRepository.GetById(id, userId);
            if (category == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            await categoryRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
