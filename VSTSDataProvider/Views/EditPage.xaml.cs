using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using VSTSDataProvider.ViewModels.ViewModelBase;

namespace VSTSDataProvider.Views;

/// <summary>
/// EditPage.xaml 的交互逻辑
/// </summary>
public partial class EditPage : UserControl
{
    public EditPage( )
    {
        InitializeComponent();
        LeftRTB.InputBindings.Add(new KeyBinding(IncreaseIndentationCommand , Key.T , ModifierKeys.Control));
        LeftRTB.InputBindings.Add(new KeyBinding(DecreaseIndentationCommand , Key.T , ModifierKeys.Control | ModifierKeys.Shift));

    }

    private ICommand _increaseIndentationCommand;
    public ICommand IncreaseIndentationCommand
    {
        get
        {
            if( _increaseIndentationCommand == null )
            {
                _increaseIndentationCommand = new RelayCommand(
                    param => IncreaseIndentationButton_Clicked(IncreaseIndentationButton , null) ,
                    param => true
                );
            }
            return _increaseIndentationCommand;
        }
    }

    private ICommand _decreaseIndentationCommand;
    public ICommand DecreaseIndentationCommand
    {
        get
        {
            if( _decreaseIndentationCommand == null )
            {
                _decreaseIndentationCommand = new RelayCommand(
                    param => DecreaseIndentationButton_Clicked(DecreaseIndentationButton , null) ,
                    param => true
                );
            }
            return _decreaseIndentationCommand;
        }
    }


    private bool RTBSaved = false;
    private static int currentNumberingNum = 1;
    private static bool isNumberingChecked = false;
    private static string textFormatCurrent = string.Empty;

    /// <summary>
    /// Generates a formatted string based on the specified single string and repetition count arrays.
    /// </summary>
    /// <param name="repeatChars">The character array to repeat.</param>
    /// <param name="numberToNextChar">The repetition count array indicating the number of characters to follow each character.</param>
    /// <param name="cycleTimes">The number of times to repeat the entire formatted string, default is 0 which means no repetition.</param>
    /// <returns>The generated formatted string.</returns>
    private string SpecifiedFormat(string[] repeatStrings , int[] numberToNextChar , int cycleTimes = 0)
    {
        // Check the validity of the input parameters
        if( repeatStrings.Length != numberToNextChar.Length || cycleTimes < 0 )
        {
            throw new ArgumentException("Invalid input parameters");
        }
        // Generate the formatted string
        string formatStr = "";
        for( int i = 0; i < repeatStrings.Length; i++ )
        {
            formatStr += string.Join("" , Enumerable.Repeat(repeatStrings[i] , numberToNextChar[i])); ;
        }

        // Repeat the entire formatted string based on the specified cycle times
        if( cycleTimes > 0 )
        {
            formatStr = new string(formatStr.ToCharArray().SelectMany(c => Enumerable.Repeat(c , cycleTimes)).ToArray());
        }

        return formatStr;
    }

    private void BulletsToggleButton_Checked(object sender , System.Windows.RoutedEventArgs e)
    {
        NumberingToggleButton.IsChecked = false;
        TextBox leftTextBoxt = (sender as ToggleButton)?.CommandTarget as TextBox;
        textFormatCurrent = SpecifiedFormat(new string[] { "\t" , "*" , " " } , new int[] { 1 , 1 , 2 });
        leftTextBoxt.PreviewKeyDown += AddSpecifiedContent;
    }

    private void BulletsToggleButton_UnChecked(object sender , System.Windows.RoutedEventArgs e)
    {
        TextBox leftTextBoxt = (sender as ToggleButton)?.CommandTarget as TextBox;
        textFormatCurrent = string.Empty;
        leftTextBoxt.PreviewKeyDown -= AddSpecifiedContent;
    }

    private void NumberingToggleButton_Checked(object sender , System.Windows.RoutedEventArgs e)
    {
        BulletsToggleButton.IsChecked = false;
        TextBox leftTextBoxt = (sender as ToggleButton)?.CommandTarget as TextBox;
        isNumberingChecked = true;
        textFormatCurrent = SpecifiedFormat(new string[] { "\t" , currentNumberingNum.ToString() , "." , " " } , new int[] { 1 , 1 , 1 , 2 });
        leftTextBoxt.PreviewKeyDown += AddSpecifiedContent;
    }

    private void NumberingToggleButton_UnChecked(object sender , System.Windows.RoutedEventArgs e)
    {
        TextBox leftTextBoxt = (sender as ToggleButton)?.CommandTarget as TextBox;
        isNumberingChecked = false;
        textFormatCurrent = string.Empty;
        currentNumberingNum = 1;
        leftTextBoxt.PreviewKeyDown -= AddSpecifiedContent;
    }

    private void AddSpecifiedContent(object sender , KeyEventArgs e)
    {
        TextBox textBox = sender as TextBox;

        // Check if the new input is a newline
        if( e.Key == Key.Enter || e.Key == Key.Return )
        {
            // Get the text before the caret
            string textBeforeCaret = textBox.Text.Substring(0 , textBox.CaretIndex);

            // Get the text after the caret
            string textAfterCaret = textBox.Text.Substring(textBox.CaretIndex);

            string textFormat = textFormatCurrent;
            // Insert a newline and a "*" at the beginning of the new line
            textBox.Text = textBeforeCaret + Environment.NewLine + textFormat + textAfterCaret;

            // Move the caret to after the "* "
            textBox.CaretIndex = textBeforeCaret.Length + textFormat.Length + 2;

            if( isNumberingChecked )
            {
                currentNumberingNum++;
                textFormatCurrent = SpecifiedFormat(new string[] { "\t" , currentNumberingNum.ToString() , "." , " " } , new int[] { 1 , 1 , 1 , 2 });
            }

            e.Handled = true; // Mark the event as handled
        }
    }

    private void IncreaseIndentationButton_Clicked(object sender , System.Windows.RoutedEventArgs e)
    {
        TextBox leftTextBox = (sender as Button)?.CommandTarget as TextBox;
        //Get the current caret position
        int caretIndex = leftTextBox.CaretIndex;
        //Get the current line
        int lineIndex = leftTextBox.GetLineIndexFromCharacterIndex(caretIndex);
        //Get the current line text
        string lineText = leftTextBox.GetLineText(lineIndex);
        //Increase Indentation from the start of the line text
        leftTextBox.Text = leftTextBox.Text.Insert(leftTextBox.GetCharacterIndexFromLineIndex(lineIndex) , "\t");
        //Set the caret index at the end of the text
        leftTextBox.CaretIndex = caretIndex + 1;
    }

    private void DecreaseIndentationButton_Clicked(object sender , System.Windows.RoutedEventArgs e)
    {
        TextBox leftTextBox = (sender as Button)?.CommandTarget as TextBox;
        //Get the current caret position
        int caretIndex = leftTextBox.CaretIndex;
        //Get the current line
        int lineIndex = leftTextBox.GetLineIndexFromCharacterIndex(caretIndex);
        //Get the current line text
        string lineText = leftTextBox.GetLineText(lineIndex);
        //Decrease Indentation from the start of the line text
        if( lineText.StartsWith("\t") )
        {
            leftTextBox.Text = leftTextBox.Text.Remove(leftTextBox.GetCharacterIndexFromLineIndex(lineIndex) , 1);
            //Set the caret index at the end of the text
            leftTextBox.CaretIndex = caretIndex - 1;
        }
    }

    private void RightSaveMenuItem_Clicked(object sender , System.Windows.RoutedEventArgs e)
    {
        RichTextBox rightRichTextBox = (sender as MenuItem)?.CommandTarget as RichTextBox;
        Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog();

        file.Filter = "Doc Files (*.doc)|*.doc|Rich Text Files (*.rtf)|*.rtf|All (*.*)|*.*";
        file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if( file.ShowDialog() == true )
        {
            System.IO.FileStream stream = new System.IO.FileStream(file.FileName , System.IO.FileMode.Create);
            TextRange range = new TextRange(rightRichTextBox.Document.ContentStart , rightRichTextBox.Document.ContentEnd);
            range.Save(stream , System.Windows.DataFormats.Rtf);
            string Title = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
            RTBSaved = true;
        }
    }

    private void RightResetMenuItem_Clicked(object sender , System.Windows.RoutedEventArgs e)
    {
        RichTextBox rightRichTextBox = (sender as MenuItem)?.CommandTarget as RichTextBox;
        ViewModels.MainWindowViewModel vm = this.DataContext as ViewModels.MainWindowViewModel;
        Paragraph paragraph = new Paragraph();
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        if( vm.IsDetailsChecked )
        {
            stringBuilder.AppendLine(@$"TestCase: {vm.EditingDetailObCollection[0].ID}");
            stringBuilder.AppendLine(@$"Title: {vm.EditingDetailObCollection[0].Name}");
            stringBuilder.AppendLine(@$"Link: {vm.EditingDetailObCollection[0].Configuration}");
            stringBuilder.AppendLine(@$"Outcome: {vm.EditingDetailObCollection[0].Outcome}");
            vm.RightEditRichTextBoxTitle = vm.EditingDetailObCollection[0].ID.ToString();
        }
        else
        {

            stringBuilder.AppendLine(@$"TestCase: {vm.EditingOTEObCollection[0].TestCaseId}");
            stringBuilder.AppendLine(@$"Title: {vm.EditingOTEObCollection[0].Title}");
            stringBuilder.AppendLine(@$"Link: {vm.EditingOTEObCollection[0].Configuration}");
            stringBuilder.AppendLine(@$"Outcome: {vm.EditingOTEObCollection[0].Outcome}");
            vm.RightEditRichTextBoxTitle = vm.EditingOTEObCollection[0].TestCaseId.ToString();
        }
        rightRichTextBox.Document.Blocks.Clear();
        paragraph.Inlines.Add(new Run(stringBuilder.ToString()));
        rightRichTextBox.Document.Blocks.Add(paragraph);
        rightRichTextBox.CaretPosition = rightRichTextBox.Document.ContentEnd;
    }

    private void RightCancelMenuItem_Click(object sender , System.Windows.RoutedEventArgs e)
    {
        RichTextBox rightRichTextBox = (sender as MenuItem)?.CommandTarget as RichTextBox;
        rightRichTextBox.Document.Blocks.Clear();
    }

    //Replaced by behavior.
    //private void EditRtfExtraMenuItem_Click(object sender , System.Windows.RoutedEventArgs e)
    //{
    //    ViewModels.MainWindowViewModel vm = this.DataContext as ViewModels.MainWindowViewModel;
    //    //Add the extra rtf to the right rich text box
    //    Paragraph paragraph = new Paragraph();
    //    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
    //    if( vm.IsDetailsChecked )
    //    {
    //        stringBuilder.AppendLine(@$"TestCase: {vm.EditingDetailObCollection[0].ID}");
    //        stringBuilder.AppendLine(@$"Title: {vm.EditingDetailObCollection[0].Name}");
    //        stringBuilder.AppendLine(@$"Link: {vm.EditingDetailObCollection[0].Configuration}");
    //        stringBuilder.AppendLine(@$"Outcome: {vm.EditingDetailObCollection[0].Outcome}");
    //    }
    //    else
    //    {

    //        stringBuilder.AppendLine(@$"TestCase: {vm.EditingOTEObCollection[0].TestCaseId}");
    //        stringBuilder.AppendLine(@$"Title: {vm.EditingOTEObCollection[0].Title}");
    //        stringBuilder.AppendLine(@$"Link: {vm.EditingOTEObCollection[0].Configuration}");
    //        stringBuilder.AppendLine(@$"Outcome: {vm.EditingOTEObCollection[0].Outcome}");
    //    }

    //    paragraph.Inlines.Add(new Run(stringBuilder.ToString()));
    //    RightRTB.Document.Blocks.Add(paragraph);
    //    RightRTB.CaretPosition = RightRTB.Document.ContentEnd;
    //}


    //private void CmbFontFamily_SelectionChanged(object sender , SelectionChangedEventArgs e)
    //{
    //    string fontName = cmbFontFamily.Text;
    //    var selection = rtbEditor.Selection;

    //    if( cmbFontFamily.SelectedItem != null )
    //        rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty , cmbFontFamily.SelectedItem);

    //    //selection.ApplyPropertyValue(TextBlock.FontFamilyProperty, new FontFamily(fontName));
    //    //TextInput 

    //    if( selection != null )
    //    {
    //        // Check whether there is text selected or just sitting at cursor
    //        if( selection.IsEmpty )
    //        {
    //            // Get current position of cursor
    //            TextPointer curCaret = rtbEditor.CaretPosition;
    //            // Get the current block object that the cursor is in
    //            Block curBlock = rtbEditor.Document.Blocks.Where
    //                (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
    //            if( curBlock != null )
    //            {
    //                Paragraph curParagraph = curBlock as Paragraph;
    //                // Create a new run object with the fontsize, and add it to the current block
    //                Run newRun = new Run();
    //                newRun.FontFamily = new System.Windows.Media.FontFamily(cmbFontFamily.SelectedItem.ToString());
    //                curParagraph.Inlines.Add(newRun);
    //                // Reset the cursor into the new block. 
    //                // If we don't do this, the font size will default again when you start typing.
    //                rtbEditor.CaretPosition = newRun.ElementStart;
    //            }
    //        }
    //    }

    //    // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
    //    rtbEditor.Focus();
    //}

    //private void CmbFontSize_SelectionChanged(object sender , SelectionChangedEventArgs e)
    //{

    //    var selection = rtbEditor.Selection;

    //    selection.ApplyPropertyValue(TextBlock.FontSizeProperty , (double)int.Parse(cmbFontSize.SelectedItem.ToString()));

    //    //selection.ApplyPropertyValue(TextBlock.FontFamilyProperty, new FontFamily(fontName));
    //    //TextInput 

    //    if( selection != null )
    //    {
    //        // Check whether there is text selected or just sitting at cursor
    //        if( selection.IsEmpty )
    //        {
    //            // Get current position of cursor
    //            TextPointer curCaret = rtbEditor.CaretPosition;
    //            // Get the current block object that the cursor is in
    //            Block curBlock = rtbEditor.Document.Blocks.Where
    //                (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
    //            if( curBlock != null )
    //            {
    //                Paragraph curParagraph = curBlock as Paragraph;
    //                // Create a new run object with the fontsize, and add it to the current block
    //                Run newRun = new Run();
    //                newRun.FontSize = (double)int.Parse(cmbFontSize.SelectedItem.ToString());
    //                curParagraph.Inlines.Add(newRun);
    //                // Reset the cursor into the new block. 
    //                // If we don't do this, the font size will default again when you start typing.
    //                rtbEditor.CaretPosition = newRun.ElementStart;
    //            }
    //        }
    //    }

    //    // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
    //    rtbEditor.Focus();
    //}

    //private void New_Executed(object sender , ExecutedRoutedEventArgs e)
    //{
    //    string text = new TextRange(rtbEditor.Document.ContentStart , rtbEditor.Document.ContentEnd).Text;
    //    if( String.IsNullOrWhiteSpace(text) )
    //    {
    //        rtbEditor.SelectAll();

    //        rtbEditor.Selection.Text = "";
    //    }
    //    else
    //    {
    //        MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to save your file?" , "New File" , MessageBoxButton.YesNoCancel);

    //        switch( result )
    //        {
    //            case MessageBoxResult.Yes:
    //                Save_Executed(null , null);
    //                saved = false;
    //                break;
    //            case MessageBoxResult.No:
    //                rtbEditor.SelectAll();
    //                rtbEditor.Selection.Text = "";
    //                saved = false;
    //                break;
    //            case MessageBoxResult.Cancel:
    //                saved = false;
    //                break;

    //        }
    //    }

    //}

    //private void Open_Executed(object sender , ExecutedRoutedEventArgs e)
    //{
    //    Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();

    //    file.Filter = file.Filter = "Doc Files (*.doc)|*.doc|Rich Text Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
    //    file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    //    if( file.ShowDialog() == true )
    //    {
    //        FileStream stream = new FileStream(file.FileName , FileMode.Open);
    //        TextRange range = new TextRange(rtbEditor.Document.ContentStart , rtbEditor.Document.ContentEnd);
    //        range.Load(stream , System.Windows.DataFormats.Rtf);
    //        Title = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
    //        saved = false;
    //    }
    //}



    //private void Exit_Executed(object sender , ExecutedRoutedEventArgs e)
    //{
    //    if( saved == false )
    //    {
    //        MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to save your file?" , "Exit Tiny Text Editor" , MessageBoxButton.YesNoCancel);

    //        switch( result )
    //        {
    //            case MessageBoxResult.Yes:
    //                Save_Executed(null , null);
    //                System.Windows.Application.Current.Shutdown();
    //                break;
    //            case MessageBoxResult.No:
    //                System.Windows.Application.Current.Shutdown();
    //                break;
    //            case MessageBoxResult.Cancel:
    //                break;
    //        }
    //    }
    //    else
    //    {
    //        System.Windows.Application.Current.Shutdown();
    //    }

    //}

    //private void rtbEditor_SelectionChanged(object sender , RoutedEventArgs e)
    //{
    //    object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
    //    btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
    //    temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
    //    btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
    //    temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
    //    btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

    //    temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
    //    cmbFontFamily.SelectedItem = temp;
    //    temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
    //    cmbFontSize.Text = temp.ToString();

    //    temp = rtbEditor.Selection.GetPropertyValue(FlowDocument.ForegroundProperty);
    //    temp = rtbEditor.Selection.GetPropertyValue(FlowDocument.BackgroundProperty);

    //    TextRange text = new TextRange(rtbEditor.Document.ContentStart , rtbEditor.Document.ContentEnd);
    //    string rtbContent = text.Text;
    //    text.Text = rtbContent.Replace("\r\n" , " ");
    //}

    //private void Toolbar_Loaded(object sender , RoutedEventArgs e)
    //{
    //    System.Windows.Controls.ToolBar toolBar = sender as System.Windows.Controls.ToolBar;
    //    var overflowGrid = toolBar.Template.FindName("OverflowGrid" , toolBar) as FrameworkElement;
    //    if( overflowGrid != null )
    //    {
    //        overflowGrid.Visibility = Visibility.Collapsed;
    //    }
    //    var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder" , toolBar) as FrameworkElement;
    //    if( mainPanelBorder != null )
    //    {
    //        mainPanelBorder.Margin = new Thickness();
    //    }
    //}

    //private void Main_Window_Closing(object sender , System.ComponentModel.CancelEventArgs e)
    //{
    //    string text = new TextRange(rtbEditor.Document.ContentStart , rtbEditor.Document.ContentEnd).Text;
    //    if( String.IsNullOrWhiteSpace(text) )
    //    {
    //        rtbEditor.SelectAll();

    //        rtbEditor.Selection.Text = "";
    //    }
    //    else if( saved == false )
    //    {
    //        MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to save your file?" , "Exit Tiny Text Editor" , MessageBoxButton.YesNoCancel);

    //        switch( result )
    //        {
    //            case MessageBoxResult.Yes:
    //                Save_Executed(null , null);
    //                System.Windows.Application.Current.Shutdown();
    //                break;
    //            case MessageBoxResult.No:
    //                System.Windows.Application.Current.Shutdown();
    //                break;
    //            case MessageBoxResult.Cancel:
    //                break;
    //        }
    //    }
    //    else
    //    {
    //        System.Windows.Application.Current.Shutdown();
    //    }
    //}

    //private void RtbEditor_TextChanged(object sender , TextChangedEventArgs e)
    //{
    //    saved = false;
    //}

    //private void BtnFontColor_Click(object sender , RoutedEventArgs e)
    //{
    //    var colorDialog = new System.Windows.Forms.ColorDialog();
    //    if( colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
    //    {
    //        var wpfColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A , colorDialog.Color.R , colorDialog.Color.G , colorDialog.Color.B);
    //        TextRange range = new TextRange(rtbEditor.Selection.Start , rtbEditor.Selection.End);
    //        range.ApplyPropertyValue(FlowDocument.ForegroundProperty , new SolidColorBrush(wpfColor));

    //        if( range != null )
    //        {
    //            // Check whether there is text selected or just sitting at cursor
    //            if( range.IsEmpty )
    //            {
    //                // Get current position of cursor
    //                TextPointer curCaret = rtbEditor.CaretPosition;
    //                // Get the current block object that the cursor is in
    //                Block curBlock = rtbEditor.Document.Blocks.Where
    //                    (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
    //                if( curBlock != null )
    //                {
    //                    Paragraph curParagraph = curBlock as Paragraph;
    //                    // Create a new run object with the fontsize, and add it to the current block
    //                    Run newRun = new Run();
    //                    newRun.Foreground = new SolidColorBrush(wpfColor);
    //                    curParagraph.Inlines.Add(newRun);
    //                    // Reset the cursor into the new block. 
    //                    // If we don't do this, the font size will default again when you start typing.
    //                    rtbEditor.CaretPosition = newRun.ElementStart;
    //                }
    //            }
    //        }

    //        rtbEditor.Focus();

    //        Color.Fill = new SolidColorBrush(wpfColor);
    //    }
    //}

    //private void BtnHighlightColor_Click(object sender , RoutedEventArgs e)
    //{
    //    var colorDialog = new System.Windows.Forms.ColorDialog();
    //    if( colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
    //    {
    //        var wpfColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A , colorDialog.Color.R , colorDialog.Color.G , colorDialog.Color.B);
    //        TextRange range = new TextRange(rtbEditor.Selection.Start , rtbEditor.Selection.End);
    //        range.ApplyPropertyValue(FlowDocument.BackgroundProperty , new SolidColorBrush(wpfColor));

    //        if( range != null )
    //        {
    //            // Check whether there is text selected or just sitting at cursor
    //            if( range.IsEmpty )
    //            {
    //                // Get current position of cursor
    //                TextPointer curCaret = rtbEditor.CaretPosition;
    //                // Get the current block object that the cursor is in
    //                Block curBlock = rtbEditor.Document.Blocks.Where
    //                    (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
    //                if( curBlock != null )
    //                {
    //                    Paragraph curParagraph = curBlock as Paragraph;
    //                    // Create a new run object with the fontsize, and add it to the current block
    //                    Run newRun = new Run();
    //                    newRun.Background = new SolidColorBrush(wpfColor);
    //                    curParagraph.Inlines.Add(newRun);
    //                    // Reset the cursor into the new block. 
    //                    // If we don't do this, the font size will default again when you start typing.
    //                    rtbEditor.CaretPosition = newRun.ElementStart;
    //                }
    //            }
    //        }

    //        rtbEditor.Focus();

    //        highlightColor.Fill = new SolidColorBrush(wpfColor);
    //    }
    //}

}
