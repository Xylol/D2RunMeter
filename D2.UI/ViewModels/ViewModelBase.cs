using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace D2.UI.ViewModels;

public interface IViewModel : INotifyPropertyChanged
{
}

public class ViewModelBase : ObservableObject, IViewModel
{
}
