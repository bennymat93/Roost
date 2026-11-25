using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Roost.Services;

namespace Roost.ViewModels;

public partial class HelpViewModel : ViewModelBase
{
    public HelpViewModel() => Reload();

    public ObservableCollection<HelpSection> Sections { get; } = new();

    [ObservableProperty] private HelpSection? _selectedSection;

    /// <summary>Rebuild the guide for the current language.</summary>
    public void Reload()
    {
        var keep = Sections.Count > 0 ? Sections.IndexOf(SelectedSection!) : 0;
        Sections.Clear();
        foreach (var s in HelpContent.Build(Loc.Language)) Sections.Add(s);
        SelectedSection = (keep >= 0 && keep < Sections.Count)
            ? Sections[keep] : Sections.FirstOrDefault();
    }
}
