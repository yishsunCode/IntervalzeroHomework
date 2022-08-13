using Autofac;
using Autofac.Features.OwnedInstances;
using Demo.Design;
using Demo.Interface;
using Demo.View;
using Demo.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.CompositionRoot
{
    static class Locator
    {
        static Lazy<IContainer> _container = new Lazy<IContainer>(Create);

        static IContainer Create()
        {
            {
                var builder = new ContainerBuilder();

                builder
                    .Register((c, p) => FakeDemoViewModelFactory.Create())
                    //.RegisterType<DemoViewModel>()
                    .As<IDemoViewModel>()
                    ;

                builder
                    .Register((c, p) => FakeInputServiceFactory.Create())
                    .As<IInputService>()
                    ;

                builder.RegisterType<DemoView>()
                    .AsSelf()
                    .OnActivated(e =>
                    {
                        var viewModel = e.Context.Resolve<IDemoViewModel>();
                        e.Instance.DataContext = viewModel;
                    })
                    ;


                return builder.Build();
            }
        }

        public static IDisposable GetView(out DemoView view) 
        {
                var resolved = _container.Value.Resolve<Owned<DemoView>>();
                view = resolved.Value;

                return resolved;
        }
    }
}
