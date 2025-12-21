using Csocsesz.Classes;

namespace Csocsesz.ContentPages;

public partial class SelectedMatchPage : ContentPage
{
	public SelectedMatchPage()
	{
		InitializeComponent();
	}
    private async void BackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}