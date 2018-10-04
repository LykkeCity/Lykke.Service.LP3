using Autofac;
using JetBrains.Annotations;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    [UsedImplicitly]
    public class AutofacModule : Module
    {
        private readonly string _walletId;

        public AutofacModule(string walletId)
        {
            _walletId = walletId;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .WithParameter(new NamedParameter("walletId", _walletId))
                .SingleInstance();
        }
    }
}
