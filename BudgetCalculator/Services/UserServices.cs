namespace BudgetCalculator.Services
{
    public interface IUserServices
    {
        public int GetUserId();
    }

    public class UserServices : IUserServices
    {
        public int GetUserId()
        {
            return 1;
        }
    }
}
