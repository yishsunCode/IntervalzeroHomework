﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Demo.Interface
{
    public interface IDemoViewModel : INotifyPropertyChanged
    {
        ICommand ChooseFolderCommand { get; }

        IList<string> ArticleList { get; }
        string SelectedArticle { get; set; }
        ICommand PracticeCommand { get; }

        IList<string> ReplayItemList { get; }
        string SelectedReplayItem { get; set; }
        ICommand ReplayCommand { get; }

        object CurrentFrame { get; }
    }

    public interface IInputFrame : INotifyPropertyChanged
    {
        string HintText { get; }
        InputFrameState State { get; }
        string UserText { get; set; }
    }
    public enum InputFrameState { Valid, Invalid, Done }
    public interface IReplayFrame : INotifyPropertyChanged
    {
        string HintText { get; }
        string ReplayingText { get; }
        ReplayFrameState State { get; }
    }
    public enum ReplayFrameState { Replaying, Done }
    public interface IWaitingFrame 
    {
        
    }
}
