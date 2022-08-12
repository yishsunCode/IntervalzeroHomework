using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Demo.Interface;
using Demo.ViewModel.Frame;
using Demo.ViewModel.Replay;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Utility.Extension;

namespace Demo.ViewModel
{
    class DemoViewModel : ObservableObject, IDemoViewModel
    {
        //TODO: IDisposable _frameDisposer = Disposable.Empty;

        IList<string> _articleList = new string[] { };
        ObservableCollection<string> _replayItemList = new();
        string _selectedArticle;
        string _selectedRelayItem;
        object _currentFrame;
        
        Subject<bool> _articleListChanged = new();
        Subject<bool> _replayListChanged = new();
        Dictionary<string, ReplayItem> _recordDict = new();

        //
        IInputService _inputService;

        public DemoViewModel(IInputService inputService)
        {
            _inputService = inputService;
            Initialize();
        }

        void Initialize()
        {
            CurrentFrame = new WaitingFrame();

            ChooseFolderCommand = new RelayCommand(() =>
            {
                ArticleList = _inputService.GetArticleList();
                SelectedArticle = ArticleList.FirstOrDefault();
            });
            PracticeCommand = ReactiveCommand.Create(() =>
            {
                var text = _inputService.GetArticle(SelectedArticle);
                var articleName = SelectedArticle;

                var frame = new InputFrame(articleName, text, item => 
                {
                    _replayItemList.Add(item.Name);
                    _replayListChanged.OnNext(default);

                    SelectedReplayItem = item.Name;

                    _recordDict.Add(item.Name, item);

                });
                CurrentFrame = frame;
            }
            , _articleListChanged.StartWith(true).Select(_ => ArticleList.Any()));

            ReplayCommand = ReactiveCommand.Create(() =>
            {
                var item = _recordDict[SelectedReplayItem];
                CurrentFrame = new ReplayFrame(item.GetReplaySequence());
            }, _replayListChanged.StartWith(true).Select(_ => ReplayItemList.Any()));

        }

        public ICommand ChooseFolderCommand { get; private set; }

        public IList<string> ArticleList
        {
            get { return _articleList; }
            private set 
            { 
                var success = this.SetProperty(ref _articleList, value);
                if (success) 
                {
                    _articleListChanged.OnNext(default);
                }
            }
        }

        public string SelectedArticle
        {
            get { return _selectedArticle; }
            set { SetProperty(ref _selectedArticle, value); }
        }

        public ICommand PracticeCommand { get; private set; }

        public IList<string> ReplayItemList => _replayItemList;

        public string SelectedReplayItem
        {
            get { return _selectedRelayItem; }
            set { SetProperty(ref _selectedRelayItem, value); }
        }

        public ICommand ReplayCommand { get; private set; }

        public object CurrentFrame
        {
            get { return _currentFrame; }
            private set { SetProperty(ref _currentFrame, value); }
        }
    }

    namespace Replay
    {

        class ReplayItem
        {
            List<ReplayRecord> _records = new();
            DateTime _prevTime;
            
            public ReplayItem(string name) 
            {
                Name = name;
            }
            public string Name { get; }
            public void Record(string text)
            {
                var now = DateTime.Now;
                var diff = _records.Any()
                    ? (int)now.Subtract(_prevTime).TotalMilliseconds
                    : 0;

                _records.Add(new ReplayRecord
                {
                    DiffMs = diff,
                    Text = text
                });
                _prevTime = now;
            }
            public IObservable<string> GetReplaySequence() 
            {
                return _records.Select(i =>
                {
                    var time = TimeSpan.FromMilliseconds(i.DiffMs);
                    return Observable.Timer(time).Select(_ => i.Text);
                })
                    .Concat();
            }
            //TODO:dispose
            public void Dispose() 
            {
                _records.Clear();
                _records = null;
            }

            class ReplayRecord
            {
                public int DiffMs { get; set; }
                public string Text { get; set; }
            }
        }
    }
    namespace Frame
    {
        class WaitingFrame : IWaitingFrame { }
        class InputFrame : ObservableObject, IInputFrame, IDisposable
        {
            CompositeDisposable _disposer = new();
            Subject<string> _userTextChanged = new();
            bool _isValid = false;
            string _userText = "";
            bool _isFinished = false;
            ReplayItem _item;

            //dependency
            string _articleName;
            string _originalText;
            Action<ReplayItem> _finished;

            public InputFrame(string articleName, string originalText, Action<ReplayItem> finished)
            {
                _articleName = articleName;
                _originalText = originalText;
                _finished = finished;
                Initialize();
            }

            public void Dispose() => _disposer.Dispose();
           
            void Initialize() 
            {
                _item = new ReplayItem(_articleName);
                Disposable.Create(() =>
                {
                    if (!_isFinished) 
                    {
                        _item.Dispose();
                    }
                });

                _userTextChanged.Subscribe(i => _item.Record(i))
                    .DisposedBy(_disposer)
                    ;

                new[]
                {
                    Observable.Return(UserText),
                    _userTextChanged
                }
                .Merge()
                .Subscribe(userText =>
                {
                    var originalSubstring = _originalText.Substring(0, userText.Length);
                    var isValid = userText == originalSubstring;
                    IsValid = isValid;

                    if (IsValid && userText.Length == _originalText.Length) 
                    {
                        _isFinished = true;
                        _finished(_item);
                    }
                })
                .DisposedBy(_disposer)
                ;

                UserText = "";

            }

            public bool IsValid 
            {
                get { return _isValid; }
                private set { this.SetProperty(ref _isValid, value); }
            }

            public string UserText 
            {
                get { return _userText; }
                set 
                { 
                    var success = this.SetProperty(ref _userText, value);
                    if (success) 
                    {
                        _userTextChanged.OnNext(_userText);
                    }
                }
            }

            public string HintText => throw new NotImplementedException();
        }
        class ReplayFrame : ObservableObject, IReplayFrame, IDisposable
        {
            CompositeDisposable _disposer = new CompositeDisposable();
            string _replayingText;

            //dependency
            IObservable<string> _replaySequence;

            public ReplayFrame(IObservable<string> replaySequence)
            {
                _replaySequence = replaySequence;
                Initialize();
            }
            void Initialize() 
            {
                _replaySequence.Subscribe(s => ReplayingText = s)
                    .DisposedBy(_disposer);
            }
            public void Dispose() => _disposer.Dispose();

            public string ReplayingText 
            {
                get { return _replayingText; }
                private set { SetProperty(ref _replayingText, value); }
            }

            public string HintText => throw new NotImplementedException();
        }
    }
}
