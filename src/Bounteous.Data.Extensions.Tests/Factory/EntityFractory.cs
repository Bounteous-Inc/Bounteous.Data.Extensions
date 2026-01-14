using Bounteous.Core.Commands;
using Bounteous.Data.Extensions.Tests.Context;
using Bounteous.xUnit.Accelerator.Factory;

namespace Bounteous.Data.Extensions.Tests.Factory;

public class EntityFractory : ICommand
{
    public void Run()
    {
        FactoryGirl
            .Define(x => x.Id, () => new TestCompany
            {
                Name = $"Company - {Guid.NewGuid().ToString()}",
                Code = "ABC-123",
                Address = "123 123 Ave Tamps Bay",
                Email = "info@advantive.com",
                Phone = "1-999-888-7777",
                IsActive = true
            });
    }
}