using System.Windows;
using System.Windows.Controls;

namespace VSTSDataProvider.Views;


/// dotnet publish -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=true --self-contained true /p:DebugType=None /p:DebugSymbols=false
// 生成一个包含所有依赖项的单个可执行文件。其中：
// -r win-x64指定目标运行时为Windows x64；
// -p:PublishSingleFile=true指定生成单个可执行文件；
// -p:IncludeNativeLibrariesForSelfExtract=true指定将Native库打包到单个可执行文件中；
// -p:PublishTrimmed=true指定使用Tree Shaking来减小发布文件的大小；
// --self-contained true指定将.NET Core运行时包含到发布文件中，以实现自包含的可执行文件；
// /p:DebugType=None /p:DebugSymbols=false指定发布版本不包含调试信息。
// 请注意，使用Tree Shaking可能会导致某些依赖项无法正常工作。
////
/// WPF 不支持 PublishTimmed 
/// 因此推荐使用如下指令:
//  不包含 Runtime, 软件体积小, 但是需要用户安装对应的 dotnet runtime.  Recommended 👇
//  dotnet publish -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained false /p:DebugType=None /p:DebugSymbols=false /p:PublishReadyToRun=true /p:PublishTrimmed=false
//  包含 Runtime, 软件体积大, 可以直接使用
//  dotnet publish -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained false /p:DebugType=None /p:DebugSymbols=false /p:PublishReadyToRun=true /p:PublishTrimmed=false


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //  DataContext 的设置迁移到APP里了
        // this.DataContext = new ViewModels.MainWindowViewModel(); 
        // 而且还要注释掉  APP.xaml 里的 <!-- StartupUri="/Views/MainWindow.xaml" --> 才不至于两个 window 窗口
    }

}

