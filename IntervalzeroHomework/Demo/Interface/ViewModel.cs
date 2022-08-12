using System.Collections.Generic;
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
        bool IsValid { get; }
        string UserText { get; set; }
    }
    public interface IReplayFrame : INotifyPropertyChanged
    {
        string HintText { get; }
        string ReplayingText { get; }
    }
    public interface IWaitingFrame 
    {
        
    }
}
