namespace AndyC.Patterns.Tests.ExampleDependencies
{
    public interface IUserService
    {
        bool VerifyPassword(int userId, string password);
        User GetUser(int userId);
        void ResetPassword(User u);
    }
}