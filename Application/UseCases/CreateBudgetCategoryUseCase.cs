using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

namespace Application.UseCases
{
    public interface ICreateBudgetCategoryUseCase
    {
        Task<CaseResult<BudgetCategoryDto?>> InvokeAsync(BudgetCategoryDto budgetcategoryDto);
    }

    public class CreateBudgetCategoryUseCase : ICreateBudgetCategoryUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public CreateBudgetCategoryUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }
        public async Task<CaseResult<BudgetCategoryDto?>> InvokeAsync(BudgetCategoryDto budgetcategoryDto)
        {
            var result = new CaseResult<BudgetCategoryDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetcategoryDto.BudgetId, userId);

                if (budget == null)
                {
                    Console.WriteLine($"Can't find budget with id {budgetcategoryDto.BudgetId} to add category.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget to add Sppending.";
                    return result;

                }

                BudgetCategory addedCategory = _UnitOfWork.BudgetCategoryRepository.CreateCategory(budgetcategoryDto.ToEntity());
                await _UnitOfWork.CommitAsync();
                result.Data = addedCategory.ToDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during creating category. Please try again later.";
            }

            return result;
        }
    }
}

