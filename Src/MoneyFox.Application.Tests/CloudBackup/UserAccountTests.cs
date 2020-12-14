using Microsoft.Graph;
using MoneyFox.Application.Common.CloudBackup;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace MoneyFox.Application.Tests.CloudBackup
{
    [ExcludeFromCodeCoverage]
    public class UserAccountTests
    {
        private UserAccount UserAccount;

        User fakeUser = new User
        {
            DisplayName = "Tested Tester",
            Mail = "test.email@email.com",
            Photo = new ProfilePhoto()
        };

        [Fact]
        public void UserAccount_Set()
        {
            UserAccount = new UserAccount();
            UserAccount.SetUserAccount(this.fakeUser);

            Assert.NotNull(UserAccount);
            Assert.Equal("Tested Tester", UserAccount.Name);
            Assert.Equal("test.email@email.com", UserAccount.Email);
            Assert.IsType<ProfilePhoto>(UserAccount.Photo);
        }

        [Fact]
        public void UserAccount_Get()
        {
            UserAccount.SetUserAccount(fakeUser);
            var testUserAccount = UserAccount.GetUserAccount();

            Assert.NotNull(testUserAccount);
            Assert.Equal("Tested Tester", UserAccount.Name);
        }
    } 
}
