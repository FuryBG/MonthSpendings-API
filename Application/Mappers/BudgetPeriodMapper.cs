using Application.Dto.Budget;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers
{
    public static class BudgetPeriodMapper
    {
        public static BudgetPeriodDto ToDto(this BudgetPeriod budgetPeriod)
        {
            return new BudgetPeriodDto()
            {
                BudgetId = budgetPeriod.BudgetId,
                StartDate = budgetPeriod.StartDate,
                EndDate = budgetPeriod.EndDate,
            };
        }
        public static BudgetPeriod ToEntity(this BudgetPeriodDto dto)
        {
            return new BudgetPeriod()
            {
                BudgetId = dto.BudgetId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
            };
        }
    }
}
