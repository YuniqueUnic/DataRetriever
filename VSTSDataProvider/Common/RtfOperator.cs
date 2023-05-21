using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace VSTSDataProvider.Common;

public class RtfOperator
{
    private RichTextBox? _richTextBox;
    private string? _currentRtfFullPath;

    public RichTextBox? RichTextBox
    {
        get { return _richTextBox; }
        set { _richTextBox = value; }
    }

    public string? CurrentRtfFullPath
    {
        get { return _currentRtfFullPath; }
        private set { _currentRtfFullPath = value; }
    }

    public RtfOperator(RichTextBox? richTextBox)
    {
        RichTextBox = richTextBox;
    }

    public void SaveRtf( )
    {
        if( RichTextBox is null )
        {
            MessageBox.Show("The RichTextBox control is not initialized." ,
                "Error" ,
                MessageBoxButton.OK ,
                MessageBoxImage.Error);
            return;
        }

        Microsoft.Win32.SaveFileDialog file = new Microsoft.Win32.SaveFileDialog();

        file.Filter = "Doc Files (*.doc)|*.doc|All (*.*)|*.*";
        file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if( file.ShowDialog() == true )
        {
            FileStream stream = new FileStream(file.FileName , FileMode.Create);
            TextRange range = new TextRange(RichTextBox.Document.ContentStart , RichTextBox.Document.ContentEnd);
            range.Save(stream , System.Windows.DataFormats.Rtf);
            CurrentRtfFullPath = file.FileName;
        }
    }

    public bool LoadRtfFrom(string rtfFileFullPath)
    {
        Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();

        file.Filter = file.Filter = "Doc Files (*.doc)|*.doc|Rich Text Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
        file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if( file.ShowDialog() == true )
        {
            FileStream stream = new FileStream(file.FileName , FileMode.Open);
            TextRange range = new TextRange(RichTextBox.Document.ContentStart , RichTextBox.Document.ContentEnd);
            range.Load(stream , System.Windows.DataFormats.Rtf);
            CurrentRtfFullPath = rtfFileFullPath;
        }

        return true;
    }

}
