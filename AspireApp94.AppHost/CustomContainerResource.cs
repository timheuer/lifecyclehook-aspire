using Aspire.Hosting.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting;
public static class CustomResourceThing
{
    public static IResourceBuilder<ContainerResource> AddCustomResource(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string? tag = null)
    {

        builder.Services.TryAddLifecycleHook<CustomResourceLifecycleHook>();

        var resource = new ContainerResource(name);

        return builder
            .AddContainer(name, "ghcr.io/microsoft/garnet", "latest")
            .WithHttpEndpoint(targetPort: 6379)
            .WithAnnotation(new ContainerImageAnnotation { Image = "ghcr.io/microsoft/garnet", Tag = "latest" });
    }

    internal sealed class CustomResourceLifecycleHook(ResourceNotificationService notificationService) : IDistributedApplicationLifecycleHook
    {
        public Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken)
        {
            var resource = appModel.Resources.OfType<ContainerResource>().FirstOrDefault(n => n.Name == "garnet");

            Task.Run(async () =>
            {
                await notificationService.PublishUpdateAsync(resource, state =>
                {
                    return state with
                    {
                        Properties = [.. state.Properties, new("TimTest", "FOO BAR")],
                        Urls = [.. state.Urls, new("test", "http://timheuer.com/blog", false)]
                    };
                });
            }, cancellationToken);

            return Task.CompletedTask;
        }
    }
}
