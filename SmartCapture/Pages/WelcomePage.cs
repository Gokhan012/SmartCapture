using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace SmartCapture;

public class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        BackgroundColor = Color.FromArgb("#0D0D0F");

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            Padding = new Thickness(48, 56, 48, 40)
        };

        var centerContent = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 0
        };

        var iconLabel = new Label
        {
            Text = "⬡",
            FontSize = 48,
            TextColor = Color.FromArgb("#5B6AF0"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 24)
        };

        var titleLabel = new Label
        {
            Text = "SmartCapture",
            FontSize = 36,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#F0F0F5"),
            HorizontalOptions = LayoutOptions.Center,
            CharacterSpacing = -0.5,
            Margin = new Thickness(0, 0, 0, 6)
        };

        var subtitleLabel = new Label
        {
            Text = "Ekran yakalama, yeniden tanımlandı.",
            FontSize = 14,
            TextColor = Color.FromArgb("#6B6B80"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 48)
        };

        var divider = new BoxView
        {
            HeightRequest = 1,
            BackgroundColor = Color.FromArgb("#1E1E28"),
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 0, 40)
        };

        var shortcutCard = new Border
        {
            BackgroundColor = Color.FromArgb("#13131A"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Stroke = Color.FromArgb("#1E1E2E"),
            StrokeThickness = 1,
            Padding = new Thickness(28, 24),
            Margin = new Thickness(0, 0, 0, 16),
            HorizontalOptions = LayoutOptions.Fill
        };

        var shortcutInner = new VerticalStackLayout { Spacing = 12 };

        var shortcutTitle = new Label
        {
            Text = "KLAVYE KISAYOLU",
            FontSize = 10,
            TextColor = Color.FromArgb("#3D3D52"),
            CharacterSpacing = 2,
            FontAttributes = FontAttributes.Bold
        };

        var shortcutRow = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center
        };

        foreach (var key in new[] { "ALT", "+", "S" })
        {
            if (key == "+")
            {
                shortcutRow.Add(new Label
                {
                    Text = "+",
                    FontSize = 18,
                    TextColor = Color.FromArgb("#3D3D52"),
                    VerticalOptions = LayoutOptions.Center
                });
            }
            else
            {
                shortcutRow.Add(new Border
                {
                    BackgroundColor = Color.FromArgb("#1A1A24"),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                    Stroke = Color.FromArgb("#2A2A3A"),
                    StrokeThickness = 1,
                    Padding = new Thickness(16, 8),
                    Content = new Label
                    {
                        Text = key,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#5B6AF0"),
                        HorizontalOptions = LayoutOptions.Center
                    }
                });
            }
        }

        var shortcutDesc = new Label
        {
            Text = "Ekranın istediğiniz alanını seçin ve yakalayın",
            FontSize = 13,
            TextColor = Color.FromArgb("#4A4A60"),
            HorizontalOptions = LayoutOptions.Center
        };

        shortcutInner.Add(shortcutTitle);
        shortcutInner.Add(shortcutRow);
        shortcutInner.Add(shortcutDesc);
        shortcutCard.Content = shortcutInner;

        var aiCard = new Border
        {
            BackgroundColor = Color.FromArgb("#0F0F18"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Stroke = Color.FromArgb("#1A1A2A"),
            StrokeThickness = 1,
            Padding = new Thickness(28, 20),
            Margin = new Thickness(0, 0, 0, 0),
            HorizontalOptions = LayoutOptions.Fill
        };

        var aiRow = new HorizontalStackLayout { Spacing = 14 };

        var aiBadge = new Border
        {
            BackgroundColor = Color.FromArgb("#1A1520"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#3D2A5A"),
            StrokeThickness = 1,
            Padding = new Thickness(8, 4),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = "YAKINDA",
                FontSize = 9,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#8B5CF6"),
                CharacterSpacing = 1.5
            }
        };

        var aiTextStack = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        aiTextStack.Add(new Label
        {
            Text = "Yapay Zeka Desteği",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#3D3D52")
        });
        aiTextStack.Add(new Label
        {
            Text = "Yakalanan alanı analiz et, açıkla, çevir ve daha fazlası",
            FontSize = 12,
            TextColor = Color.FromArgb("#2A2A3A")
        });

        aiRow.Add(aiBadge);
        aiRow.Add(aiTextStack);
        aiCard.Content = aiRow;

        centerContent.Add(iconLabel);
        centerContent.Add(titleLabel);
        centerContent.Add(subtitleLabel);
        centerContent.Add(divider);
        centerContent.Add(shortcutCard);
        centerContent.Add(aiCard);

        var hideButton = new Button
        {
            Text = "Arka Plana Al ve Başlat",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            WidthRequest = 280,
            HeightRequest = 52,
            CornerRadius = 14,
            BackgroundColor = Color.FromArgb("#5B6AF0"),
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            CharacterSpacing = 0.3
        };

        hideButton.Clicked += (s, e) =>
        {
#if WINDOWS
            var nativeWindow = this.Window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            nativeWindow?.GetAppWindow()?.Hide();
#endif
        };

        Grid.SetRow(centerContent, 0);
        Grid.SetRow(hideButton, 1);

        root.Add(centerContent);
        root.Add(hideButton);

        Content = root;
    }
}