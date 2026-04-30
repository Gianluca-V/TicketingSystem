using Microsoft.AspNetCore.Identity;
using Moq;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.Interfaces.persistence;

namespace TicketingSystem.UnitTests.Helpers;

public static class MockHelpers
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }

    public static Mock<ICacheService> MockCacheService()
    {
        return new Mock<ICacheService>();
    }

    public static Mock<IUnitOfWork> MockUnitOfWork()
    {
        return new Mock<IUnitOfWork>();
    }
}
