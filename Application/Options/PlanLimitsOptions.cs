namespace Application.Options
{
    public class PlanLimitsOptions
    {
        public int FreeBudgetLimit { get; set; } = 1;
        public int ProBudgetLimit { get; set; } = 3;
        public int FreeParticipantJoinThreshold { get; set; } = 1;
        public int ProParticipantLimit { get; set; } = 10;
    }
}
