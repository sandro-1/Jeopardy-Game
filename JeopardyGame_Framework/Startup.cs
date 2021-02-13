using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JeopardyGame_Framework.Startup))]
namespace JeopardyGame_Framework
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
