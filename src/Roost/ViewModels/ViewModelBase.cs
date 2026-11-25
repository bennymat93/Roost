using CommunityToolkit.Mvvm.ComponentModel;
using Roost.Services;

namespace Roost.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public Localizer Loc => Localizer.Instance;
}
