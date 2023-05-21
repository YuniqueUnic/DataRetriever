using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace VSTSDataProvider.Common.UIElementActions;

class MyBehaviors { }

#region Propdp Behaviors

public class RefreshCollectionViewBehavior : Behavior<FrameworkElement>
{
    public ICollectionView TargetCollectionView
    {
        get { return (ICollectionView)GetValue(TargetCollectionViewProperty); }
        set { SetValue(TargetCollectionViewProperty , value); }
    }

    // Using a DependencyProperty as the backing store for targetCollectionView.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TargetCollectionViewProperty =
        DependencyProperty.Register(
            "TargetCollectionView" ,
            typeof(ICollectionView) ,
            typeof(RefreshCollectionViewBehavior) ,
            new PropertyMetadata(null));

    protected override void OnAttached( )
    {
        base.OnAttached();
        AssociatedObject.PreviewKeyDown += RefreshCollectionView;
    }

    protected override void OnDetaching( )
    {
        base.OnDetaching();
        AssociatedObject.PreviewKeyDown -= RefreshCollectionView;
    }

    private void RefreshCollectionView(object sender , KeyEventArgs e)
    {
        if( TargetCollectionView == null ) return;

        if( e.Key == Key.Enter )
        {
            TargetCollectionView.Refresh();
        }
    }

}

public class DoubleClickToEditItemBehavior : Behavior<FrameworkElement>
{
    public IList TargetEditingList
    {
        get { return (IList)GetValue(TargetEditingListProperty); }
        set { SetValue(TargetEditingListProperty , value); }
    }
    // Using a DependencyProperty as the backing store for TargetEditingList.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TargetEditingListProperty =
        DependencyProperty.Register(
                       "TargetEditingList" ,
                            typeof(IList) ,
                            typeof(DoubleClickToEditItemBehavior) ,
                            new PropertyMetadata(null));

    protected override void OnAttached( )
    {
        base.OnAttached();
        AssociatedObject.MouseLeftButtonDown += EditItem;
    }
    protected override void OnDetaching( )
    {
        base.OnDetaching();
        AssociatedObject.MouseLeftButtonDown -= EditItem;
    }

    private void EditItem(object sender , MouseButtonEventArgs e)
    {
        if( TargetEditingList == null || sender == null ) return;

        DataGrid dataGrid = sender as DataGrid;

        if( dataGrid.SelectedItem == null ) return;

        if( e.ClickCount == 2 )
        {
            TargetEditingList.Clear();
            TargetEditingList.Add(dataGrid.SelectedItem);
        }
    }

}

// TODO: This behavior is not complete.
public class EditinTheSideOfBehavior : Behavior<FrameworkElement>
{
    // public string SideName
    // {
    //     get { return (string)GetValue(SideNameProperty); }
    //     set { SetValue(SideNameProperty , value); }
    // }

    public string RichTextBoxTitle
    {
        get { return (string)GetValue(RichTextBoxTitleProperty); }
        set { SetValue(RichTextBoxTitleProperty , value); }
    }

    public string RichTextBoxContent
    {
        get { return (string)GetValue(RichTextBoxContentProperty); }
        set { SetValue(RichTextBoxContentProperty , value); }
    }

    // Using a DependencyProperty as the backing store for SideName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SideNameProperty =
        DependencyProperty.Register(
            "SideName" ,
            typeof(string) ,
            typeof(EditinTheSideOfBehavior) ,
            new PropertyMetadata("Left"));

    // Using a DependencyProperty as the backing store for RichTextBoxTitle.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RichTextBoxTitleProperty =
        DependencyProperty.Register(
            "RichTextBoxTitle" ,
            typeof(string) ,
            typeof(EditinTheSideOfBehavior) ,
            new FrameworkPropertyMetadata("RichTextBox" , FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    // Using a DependencyProperty as the backing store for RichTextBoxContent.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RichTextBoxContentProperty =
        DependencyProperty.Register(
            "RichTextBoxContent" ,
            typeof(string) ,
            typeof(EditinTheSideOfBehavior) ,
            new FrameworkPropertyMetadata((new FlowDocument(
                new Paragraph(new Run(string.Empty)))).ToString() ,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    protected override void OnAttached( )
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += EditinTheSideof;
    }

    private void EditinTheSideof(object sender , MouseButtonEventArgs e)
    {

        if( sender == null ) return;
        // if( string.IsNullOrEmpty(SideName) ) return;
        try
        {

            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem?.Parent as ContextMenu;
            DataGridCell dataGridCell = contextMenu?.PlacementTarget as DataGridCell;
            //DataGridCell dataGridCell = ((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as DataGridCell;
            DataGridColumn dataGridTextColumn = dataGridCell?.Column;
            string cellValue = (dataGridCell.Content as TextBlock).Text;
            string cellColumnHeaderValue = dataGridTextColumn.Header.ToString();

            RichTextBoxTitle = cellColumnHeaderValue;
            RichTextBoxContent = cellValue;

            BindingOperations.GetBindingExpression(this , RichTextBoxTitleProperty).UpdateSource();
            BindingOperations.GetBindingExpression(this , RichTextBoxContentProperty).UpdateSource();
        }
        catch( NullReferenceException )
        {
            throw;
        }

    }

    protected override void OnDetaching( )
    {
        base.OnDetaching();
        AssociatedObject.PreviewMouseLeftButtonDown -= EditinTheSideof;
    }

}

#endregion Propdp Behaviors

#region PropAP AttachProperty

public class BindingProxy : Freezable
{
    #region Overrides of Freezable

    protected override Freezable CreateInstanceCore( )
    {
        return new BindingProxy();
    }

    #endregion

    public object Data
    {
        get { return (object)GetValue(DataProperty); }
        set { SetValue(DataProperty , value); }
    }

    // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data" ,
            typeof(object) ,
            typeof(BindingProxy) ,
            new UIPropertyMetadata(null));
}

public class RichTextBoxHelper : DependencyObject
{
    public static string GetDocumentXaml(DependencyObject obj)
    {
        return (string)obj.GetValue(DocumentXamlProperty);
    }

    public static void SetDocumentXaml(DependencyObject obj , string value)
    {
        obj.SetValue(DocumentXamlProperty , value);
    }
    // BindsTwoWayByDefault = 
    // PropertyChangedCallback = 
    public static readonly DependencyProperty DocumentXamlProperty =
        DependencyProperty.RegisterAttached(
            "DocumentXaml" ,
            typeof(string) ,
            typeof(RichTextBoxHelper) ,
            new FrameworkPropertyMetadata("" ,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ,
                (obj , e) =>
                {
                    var richTextBox = (RichTextBox)obj;

                    // Parse the XAML to a document (or use XamlReader.Parse())
                    var xaml = GetDocumentXaml(richTextBox);
                    //if( xaml.IsNullOrWhiteSpaceOrEmpty() ) { xaml = " "; }
                    var doc = new FlowDocument();
                    var range = new TextRange(doc.ContentStart , doc.ContentEnd);

                    range.Load(new MemoryStream(Encoding.UTF8.GetBytes(xaml)) ,
                          DataFormats.Rtf);

                    // Set the document
                    richTextBox.Document = doc;

                    // When the document changes update the source
                    range.Changed += (obj2 , e2) =>
                                    {
                                        if( richTextBox.Document == doc )
                                        {
                                            MemoryStream buffer = new MemoryStream();
                                            range.Save(buffer , DataFormats.Rtf);
                                            SetDocumentXaml(richTextBox ,
                                                Encoding.UTF8.GetString(buffer.ToArray()));
                                        }
                                    };
                }
            ));
}

public class BindingINFOHelper : DependencyObject
{

    public static object GetBindingObj(DependencyObject obj)
    {
        return (object)obj.GetValue(BindingObjProperty);
    }

    public static void SetBindingObj(DependencyObject obj , object value)
    {
        obj.SetValue(BindingObjProperty , value);
    }

    // Using a DependencyProperty as the backing store for BindingObj.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingObjProperty =
        DependencyProperty.RegisterAttached("BindingObj" ,
            typeof(object) ,
            typeof(BindingINFOHelper) ,
            new FrameworkPropertyMetadata(null , FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


    public static object GetBindingObj2(DependencyObject obj)
    {
        return (object)obj.GetValue(BindingObj2Property);
    }

    public static void SetBindingObj2(DependencyObject obj , object value)
    {
        obj.SetValue(BindingObj2Property , value);
    }

    // Using a DependencyProperty as the backing store for BindingObj2.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingObj2Property =
        DependencyProperty.RegisterAttached(
            "BindingObj2" ,
            typeof(object) ,
            typeof(BindingINFOHelper) ,
            new FrameworkPropertyMetadata(null , FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

}


#endregion PropAP AttachProperty