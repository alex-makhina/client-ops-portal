using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests
{
    [CollectionDefinition("SubscriptionHistory Integration Tests", DisableParallelization = true)]
    public class IntegrationTestCollection : ICollectionFixture<TestFixture>
    {
        // Этот класс служит маркером для коллекции тестов
    }
}