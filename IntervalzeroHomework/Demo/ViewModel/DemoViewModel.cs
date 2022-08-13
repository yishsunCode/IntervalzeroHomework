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
                    var name = articleName;

                    for (int i = 1;  _recordDict.ContainsKey(name); i++)
                    {
                        name = $"{articleName}_{i}";
                    }

                    item.Name = name;
                    _recordDict.Add(name, item);

                    //update target item
                    SelectedReplayItem = name;

                    //update list
                    _replayItemList.Add(name);
                    _replayListChanged.OnNext(default);

                });
                CurrentFrame = frame;
            }
            , _articleListChanged.StartWith(true).Select(_ => ArticleList.Any()));

            ReplayCommand = ReactiveCommand.Create(() =>
            {
                var item = _recordDict[SelectedReplayItem];
                CurrentFrame = new ReplayFrame(item.OriginalText, item.GetReplaySequence());
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
            
            public ReplayItem(string originalText) 
            {
                OriginalText = originalText;
            }
            public string OriginalText { get; }
            public string Name { get; set; }
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
            string _userText = "";
            bool _isFinished = false;
            ReplayItem _item;
            string _hintText;
            InputFrameState _state;

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
                _item = new ReplayItem(_originalText);
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
                    var originalSubstring = _originalText.Substring(0, Math.Min(userText.Length, _originalText.Length));
                    var valid = userText == originalSubstring ? InputFrameState.Valid : InputFrameState.Invalid;
                    State = valid;

                    //Hint Text
                    HintText = _originalText.Substring(Math.Min(_originalText.Length, userText.Length));

                    if (InputFrameState.Valid == State 
                        && userText.Length == _originalText.Length) 
                    {
                        State = InputFrameState.Done;
                        _isFinished = true;
                        _finished(_item);
                    }
                })
                .DisposedBy(_disposer)
                ;

                UserText = "";

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

            public string HintText 
            {
                get { return _hintText; }
                private set { SetProperty(ref _hintText, value); }
            }
            public InputFrameState State
            {
                get { return _state; }
                private set { SetProperty(ref _state, value); }
            }
        }
        class ReplayFrame : ObservableObject, IReplayFrame, IDisposable
        {
            CompositeDisposable _disposer = new CompositeDisposable();
            string _replayingText;
            string _hintText;
            ReplayFrameState _state;

            //dependency
            string _originalText;
            IObservable<string> _replaySequence;

            public ReplayFrame(string originalText, IObservable<string> replaySequence)
            {
                _originalText = originalText;
                _replaySequence = replaySequence;
                Initialize();
            }
            void Initialize() 
            {
                State = ReplayFrameState.Replaying;
                _replaySequence.Subscribe(s => 
                {
                    ReplayingText = s;

                    //Hint Text
                    HintText = _originalText.Substring(Math.Min(_originalText.Length, ReplayingText.Length));
                }, () =>
                {
                    State = ReplayFrameState.Done;
                })
                    .DisposedBy(_disposer);
            }
            public void Dispose() => _disposer.Dispose();

            public string ReplayingText 
            {
                get { return _replayingText; }
                private set { SetProperty(ref _replayingText, value); }
            }
            public string HintText 
            {
                get { return _hintText; }
                private set { SetProperty(ref _hintText, value); }
            }

            public ReplayFrameState State
            {
                get { return _state; }
                private set { SetProperty(ref _state, value); }
            }
        }
    }
}
