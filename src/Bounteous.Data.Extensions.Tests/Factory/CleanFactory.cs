using Bounteous.Core.Commands;
using Bounteous.xUnit.Accelerator.Factory;

namespace Bounteous.Data.Extensions.Tests.Factory;

public class CleanFactory : ICommand
{
    public void Run() => FactoryGirl.Clear();
}