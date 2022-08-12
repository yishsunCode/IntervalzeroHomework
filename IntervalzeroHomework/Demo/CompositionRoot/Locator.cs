﻿using Autofac;
using Autofac.Features.OwnedInstances;
using Demo.Interface;
using Demo.View;
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
                    //.RegisterType<IDemoViewModel>()
                    .Register((c, p) => FakeDemoViewModelFactory.Create())
                    .As<IDemoViewModel>()
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
