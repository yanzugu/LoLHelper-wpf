using LoLHelper.Src;
using Stylet;
using StyletIoC;

namespace LoLHelper
{
    public class Bootstrapper : Bootstrapper<LoLHelperViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}
