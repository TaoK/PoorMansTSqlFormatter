using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace PoorMansTSqlFormatterVSIXPROJECT
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PoorMansTSqlFormatterVSIXPROJECTPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class PoorMansTSqlFormatterVSIXPROJECTPackage : AsyncPackage
    {
        public const string PackageGuidString = "0b08762c-20ba-42de-9e99-916579184d4f";
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await MyCommand.InitializeAsync(this);
        }
    }
}
