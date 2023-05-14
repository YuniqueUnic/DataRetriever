using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VSTSDataProvider.Common.UIElementActions;

class MyBehaviors { }

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
