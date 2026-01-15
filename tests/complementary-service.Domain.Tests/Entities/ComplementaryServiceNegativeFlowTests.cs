using System;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.Entities
{
    public class ComplementaryServiceNegativeFlowTests
    {
        [Fact]
        public void Confirm_WhenServiceIsCancelled_ShouldThrowInvalidOperationException()
        {
            var service = new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Catering, "D");
            service.Cancel();

            var response = new ProviderResponse(true, "p1", "ok", 10m);

            Assert.Throws<InvalidOperationException>(() => service.Confirm(response));
        }

        [Fact]
        public void MarkAsPending_WhenStatusNotRequested_Throws()
        {
            var service = new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Catering, "D");
            service.Cancel();

            Assert.Throws<InvalidOperationException>(() => service.MarkAsPending("p1"));
        }
    }
}
