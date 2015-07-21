using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TrolleyTracker.Startup))]
namespace TrolleyTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
