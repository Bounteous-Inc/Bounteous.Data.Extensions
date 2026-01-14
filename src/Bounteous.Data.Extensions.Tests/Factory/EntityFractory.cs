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
            })
            
            .Define(x => x.Id, () => new ProjectStatus
                { Name = "Not Started", IsDefault = true })
            
            .Define(x => x.Id, () =>
            {
                var status = FactoryGirl.Build<ProjectStatus>();
                return new TestProject
                {
                    Name = $"Project - {Guid.NewGuid().ToString()}",
                    Code = "ABC-123",
                    Status = status,
                    Company = FactoryGirl.Build<TestCompany>(),
                    StatusId = status.Id
                };
            });
    }
}