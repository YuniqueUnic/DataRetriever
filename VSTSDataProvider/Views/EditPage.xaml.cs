using System.Windows.Controls;

namespace VSTSDataProvider.Views;

/// <summary>
/// EditPage.xaml 的交互逻辑
/// </summary>
public partial class EditPage : UserControl
{
    public EditPage( )
    {
        InitializeComponent();
    }

    //private void rtbEditor_SelectionChanged(object sender , RoutedEventArgs e)
    //{
    //    object temp = RightRTB.Selection.GetPropertyValue(Inline.FontWeightProperty);
    //    //BoldToggleButton.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
    //    //temp = RightRTB.Selection.GetPropertyValue(Inline.FontStyleProperty);
    //    //ItalicToggleButton.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
    //    //temp = RightRTB.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
    //    //UnderlineToggleButton.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

    //    //temp = RightRTB.Selection.GetPropertyValue(Inline.FontFamilyProperty);
    //    //FontFamilyComboBox.SelectedItem = temp;
    //    //temp = RightRTB.Selection.GetPropertyValue(Inline.FontSizeProperty);
    //    //FontSizeComboBox.Text = temp.ToString();

    //    temp = RightRTB.Selection.GetPropertyValue(FlowDocument.ForegroundProperty);
    //    temp = RightRTB.Selection.GetPropertyValue(FlowDocument.BackgroundProperty);

    //    TextRange text = new TextRange(RightRTB.Document.ContentStart , RightRTB.Document.ContentEnd);
    //    string rtbContent = text.Text;
    //    text.Text = rtbContent.Replace("\r\n" , " ");
    //}
}
