using System.Windows;
using System.Windows.Controls;
using AuctionApp.Services;

namespace AuctionApp.Views;

/// <summary>A themed replacement for MessageBox with up to three custom buttons.</summary>
public partial class MessageDialog : Window
{
    public MessageDialog(string title, string message, string primary, string? secondary, string? cancel)
    {
        InitializeComponent();
        Title = title;
        HeadingText.Text = title;
        MessageText.Text = message;

        AddButton(primary, DialogChoice.Primary, isDefault: true, isCancel: cancel == null && secondary == null);
        if (secondary != null)
        {
            AddButton(secondary, DialogChoice.Secondary, isDefault: false, isCancel: cancel == null);
        }

        if (cancel != null)
        {
            AddButton(cancel, DialogChoice.Cancel, isDefault: false, isCancel: true);
        }
    }

    public DialogChoice Choice { get; private set; } = DialogChoice.Cancel;

    private void AddButton(string text, DialogChoice choice, bool isDefault, bool isCancel)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 96,
            Margin = new Thickness(ButtonPanel.Children.Count == 0 ? 0 : 8, 0, 0, 0),
            IsDefault = isDefault,
            IsCancel = isCancel,
        };
        if (isDefault)
        {
            button.SetResourceReference(StyleProperty, "AccentButtonStyle");
        }

        button.Click += (_, _) =>
        {
            Choice = choice;
            if (!isCancel)
            {
                // Cancel buttons close the dialog by themselves.
                DialogResult = true;
            }
        };
        ButtonPanel.Children.Add(button);
    }
}
