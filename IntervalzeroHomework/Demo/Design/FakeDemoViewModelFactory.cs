using CommunityToolkit.Mvvm.Input;
using Demo.Interface;
using NSubstitute;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;

namespace Demo.Design
{
    public class FakeDemoViewModelFactory
    {
        public static IDemoViewModel Create() 
        {
            var vm = Substitute.For<IDemoViewModel>();


            var chooseFolderCommand = new RelayCommand(() =>
            {
                MessageBox.Show(nameof(IDemoViewModel.ChooseFolderCommand));
            });
            var practiceCommand = new RelayCommand(() =>
            {
                MessageBox.Show(nameof(IDemoViewModel.PracticeCommand));
            });
            var replayCommand = new RelayCommand(() =>
            {
                MessageBox.Show(nameof(IDemoViewModel.ReplayCommand));
            });

            vm.ChooseFolderCommand.Returns(chooseFolderCommand);
            vm.PracticeCommand.Returns(practiceCommand);
            vm.ReplayCommand.Returns(replayCommand);

            //
            var articleList = Enumerable.Range(0, 5).Select(i => $"Article{i}").ToArray();
            var replayList = Enumerable.Range(10, 20).Select(i => $"Replay{i}").Reverse().ToArray();

            vm.ArticleList.Returns(articleList);
            vm.SelectedArticle.Returns(articleList[3]);
            vm.ReplayItemList.Returns(replayList);
            vm.SelectedReplayItem.Returns(replayList[5]);

            //frame
            var frameChangingTime = TimeSpan.FromSeconds(3);
            var validChangingTime = TimeSpan.FromSeconds(1);
            var replayAppendTime = TimeSpan.FromSeconds(0.5);

            var frameChangingTicks = new[]
            {
                Observable.Return(false),
                Observable.Interval(frameChangingTime).Select(_ => false)
            }.Merge()
            .Scan(-1, (acc, _) => acc + 1)
            ;

            frameChangingTicks
                .Do(_ => { })
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(i =>
            {
                var mod = i % 3;
                if (0 == mod)
                {
                    var frame = Substitute.For<IWaitingFrame>();

                    vm.CurrentFrame.Returns(frame);
                    vm.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(vm, new PropertyChangedEventArgs(nameof(vm.CurrentFrame)));

                }
                else if (1 == mod)
                {
                    var frame = Substitute.For<IInputFrame>();
                    
                    //
                    var ticks = new[]
                    {
                        Observable.Return(true),
                        Observable.Interval(validChangingTime).Select(i => i % 2 != 0),
                    }.Merge();

                    ticks
                        .ObserveOn(SynchronizationContext.Current)
                        .Subscribe(valid =>
                        {
                            frame.IsValid.Returns(valid);
                            frame.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(frame, new PropertyChangedEventArgs(nameof(frame.IsValid)));
                        });
                    //
                    frame.UserText.Returns("UserInput...");
                    frame.HintText.Returns("HINT~hint~HintHINT");

                    vm.CurrentFrame.Returns(frame);
                    vm.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(vm, new PropertyChangedEventArgs(nameof(vm.CurrentFrame)));

                }
                else if (2 == mod)
                {
                    var frame = Substitute.For<IReplayFrame>();

                    var appends = new[]
                    {
                        "Test_", "Replay_", "Function_", "Now_"
                    };

                    var ticks = Observable.Interval(replayAppendTime)
                    .Select(i => (int)i)
                    .Select(i => appends[i])
                    .Take(appends.Length)
                    ;

                    ticks
                        .ObserveOn(SynchronizationContext.Current)
                        .Subscribe(word =>
                        {
                            var appended = frame.ReplayingText + word;
                            frame.ReplayingText.Returns(appended);
                            frame.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(frame, new PropertyChangedEventArgs(nameof(frame.ReplayingText)));
                        });

                    //
                    frame.ReplayingText.Returns("Replay...");
                    frame.HintText.Returns("HINT~hint~HintHINT");
                    vm.CurrentFrame.Returns(frame);
                    vm.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(vm, new PropertyChangedEventArgs(nameof(vm.CurrentFrame)));
                }
            });
            
            return vm;
        }
    }
}
