namespace Csocsesz.ContentViews;

// One team's picture(s). With a second picture both are shown smaller and fanned out like two cards:
// the first player's picture in front on the right, the mate's behind on the left.
public class TeamPictures : ContentView
{
    private readonly Image first = new() { Aspect = Aspect.AspectFit };
    private readonly Image second = new() { Aspect = Aspect.AspectFit, IsVisible = false };

    public static readonly BindableProperty Source1Property = BindableProperty.Create(nameof(Source1), typeof(string), typeof(TeamPictures), null,
        propertyChanged: (b, _, _) => ((TeamPictures)b).Refresh());
    public static readonly BindableProperty Source2Property = BindableProperty.Create(nameof(Source2), typeof(string), typeof(TeamPictures), null,
        propertyChanged: (b, _, _) => ((TeamPictures)b).Refresh());
    // Height of a single picture; pictures shrink to 3/4 of it when two are shown.
    public static readonly BindableProperty PictureHeightProperty = BindableProperty.Create(nameof(PictureHeight), typeof(double), typeof(TeamPictures), 80.0,
        propertyChanged: (b, _, _) => ((TeamPictures)b).Refresh());

    public string? Source1 { get => (string?)GetValue(Source1Property); set => SetValue(Source1Property, value); }
    public string? Source2 { get => (string?)GetValue(Source2Property); set => SetValue(Source2Property, value); }
    public double PictureHeight { get => (double)GetValue(PictureHeightProperty); set => SetValue(PictureHeightProperty, value); }

    public TeamPictures()
    {
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        // Added last = drawn on top.
        Content = new Grid { Children = { second, first } };
        Refresh();
    }

    private void Refresh()
    {
        bool two = !string.IsNullOrEmpty(Source2);
        double h = two ? PictureHeight * 0.75 : PictureHeight;
        first.Source = Source1;
        second.Source = Source2;
        second.IsVisible = two;
        first.HeightRequest = second.HeightRequest = h;
        first.Rotation = two ? 10 : 0;
        second.Rotation = -10;
        first.TranslationX = two ? h * 0.35 : 0;
        second.TranslationX = -h * 0.35;
        first.TranslationY = two ? h * 0.06 : 0;
        second.TranslationY = -h * 0.06;
    }
}
