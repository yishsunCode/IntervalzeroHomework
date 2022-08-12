using System;
using System.Reactive.Disposables;

namespace Utility.Extension
{
    public static class ExtensionMethods
    {
        public static void DisposedBy(this IDisposable disposer, CompositeDisposable composite) 
        {
            composite.Add(disposer);
        }
    }
}
