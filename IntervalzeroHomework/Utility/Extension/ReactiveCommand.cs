using GalaSoft.MvvmLight.Command;
using System;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace Utility.Extension
{
    public static class ReactiveCommand
    {
        public static ICommand Create(Action action, IObservable<bool> conditionSequence) 
        {
            bool localCondition = true;

            var command = new RelayCommand(action, () => localCondition);
            conditionSequence.Subscribe(cond =>
            {
                localCondition = cond;
                command.RaiseCanExecuteChanged();
            });

            return command;
        }
    }
}
