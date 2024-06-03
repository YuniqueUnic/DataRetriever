using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using VSTSDataProvider.Common.Helpers;
using VSTSDataProvider.ViewModels.ViewModelBase;

namespace VSTSDataProvider.ViewModels;

public class AboutViewModel : ViewModelBase.BaseViewModel
{
    public AboutViewModel( )
    {
        GetSoftwareAssemblyInfos();
        AboutContentFlowDocumentReader_LoadedCommand = new RelayCommand(AboutContentFlowDocumentReader_Loaded);
        OKButtonClickedCommand = new RelayCommand(ClosethisWindow);
    }

    private void GetSoftwareAssemblyInfos( )
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        AssemblyName assemblyName = assembly.GetName();

        string name = assemblyName.Name ?? "";
        string version = (assemblyName.Version ?? new Version("0.0.0.0")).ToString();
        string copyright = string.Empty;
        string releaseDate = string.Empty;

        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute) , false);
        if( attributes.Length > 0 )
        {
            AssemblyTrademarkAttribute trademarkAttribute = (AssemblyTrademarkAttribute)attributes[0];
            copyright = trademarkAttribute.Trademark;
        }

        attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute) , false);
        if( attributes.Length > 0 )
        {
            AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)attributes[0];
            releaseDate = informationalVersionAttribute.InformationalVersion.Split('+')[0];
        }

        _softwareName = name;
        _softwareVersion = version;
        _copyrightOwner = copyright;
        _releaseDate = releaseDate;
    }

    public ICommand AboutContentFlowDocumentReader_LoadedCommand { get; set; }
    public ICommand OKButtonClickedCommand { get; set; }

    //TODO: Violated the MVVM design pattern and will be replaced with behavior in the future.
    private void AboutContentFlowDocumentReader_Loaded(object sender)
    {
        FlowDocumentReader flowDocumentReader = (FlowDocumentReader)sender;
        FlowDocument flowDocument = GenerateLicenseInfoDocument();
        flowDocumentReader.Document = flowDocument;
    }

    //TODO: Violated the MVVM design pattern and will be replaced with behavior in the future.
    private void ClosethisWindow(object window) => (window as Window)!.Close();

    #region FlowDocumentContent

    private string _softwareName = nameof(_softwareName);
    private string _softwareVersion = nameof(_softwareVersion);
    private string _copyrightOwner = nameof(_copyrightOwner);
    private string _releaseDate = nameof(_releaseDate);
    private List<Models.OpenSourceProjectInfos> _openSourceLibraries = new List<Models.OpenSourceProjectInfos>
    {
        new Models.OpenSourceProjectInfos { Name = "Microsoft.Xaml.Behaviors.Wpf", Version = "1.1.39", License = "MIT", Url = "https://github.com/Microsoft/XamlBehaviorsWpf" },
        new Models.OpenSourceProjectInfos { Name = "MiniExcel", Version = "1.30.3", License = "Apache-2.0", Url = "https://github.com/mini-software/MiniExcel" },
        new Models.OpenSourceProjectInfos { Name = "Newtonsoft.Json", Version = "13.0.3", License = "MIT", Url = "https://www.newtonsoft.com/json" }
    };
    private List<Models.OpenSourceProjectInfos> _thirdPartyComponents = new List<Models.OpenSourceProjectInfos>
    {
        new Models.OpenSourceProjectInfos { Name = "ICON", Version = "", License = "", Url = "https://icons8.com/" },
        //new Models.OpenSourceProjectInfos { Name = "第三方组件名称2", Version = "第三方组件版本号2", License = "第三方组件许可证2", Url = "http://Bing.com" },
        //new Models.OpenSourceProjectInfos { Name = "第三方组件名称3", Version = "第三方组件版本号3", License = "第三方组件许可证3", Url = "http://Google.com" }
    };
    private List<Models.OpenSourceProjectInfos> _specialThanks = new List<Models.OpenSourceProjectInfos>
    {
            new Models.OpenSourceProjectInfos { Name = "HYSYS", Version = "", License = "", Url = "https://www.aspentech.com/en/products/engineering/aspen-hysys/?src=web-apaccn" },
            new Models.OpenSourceProjectInfos { Name = "AspenTech", Version = "", License = "", Url = "https://www.aspentech.cn/" },
    };

    public string SoftwareName
    {
        get => _softwareName;
        set => SetProperty(ref _softwareName , value);
    }

    public string SoftwareVersion
    {
        get => _softwareVersion;
        set => SetProperty(ref _softwareVersion , value);
    }

    public string CopyrightOwner
    {
        get => _copyrightOwner;
        set
        => SetProperty(ref _copyrightOwner , value);
    }

    public string ReleaseDate
    {
        get => _releaseDate;
        set
        => SetProperty(ref _releaseDate , value);
    }

    public List<Models.OpenSourceProjectInfos> OpenSourceLibraries
    {
        get => _openSourceLibraries;
        set
        => SetProperty(ref _openSourceLibraries , value);
    }

    public List<Models.OpenSourceProjectInfos> ThirdPartyComponents
    {
        get => _thirdPartyComponents;
        set
        => SetProperty(ref _thirdPartyComponents , value);
    }

    public List<Models.OpenSourceProjectInfos> SpecialThanks
    {
        get => _specialThanks;
        set
        => SetProperty(ref _specialThanks , value);
    }

    #region Rewrite to AboutContentRichTextBox_Loaded()

    #region Some great infos
    // In WPF, the Document property of RichTextBox is read-only, so we cannot directly bind a FlowDocument to it.
    // Instead, we can set the FlowDocument to the Document property of RichTextBox using code-behind in the Loaded event handler.
    //private void AboutContentRichTextBox_Loaded(object sender , RoutedEventArgs e)
    //{
    //    RichTextBox richTextBox = (RichTextBox)sender;
    //    LicenseInfoViewModel viewModel = (LicenseInfoViewModel)this.DataContext;
    //    FlowDocument flowDocument = viewModel.GenerateLicenseInfoDocument();
    //    richTextBox.Document = flowDocument;
    //}
    // When the RichTextBox control is loaded, the AboutContentRichTextBox_Loaded event handler is triggered.
    // In this handler, we get the LicenseInfoViewModel instance that is bound to the DataContext of the window,
    // call its GenerateLicenseInfoDocument method to generate a FlowDocument, and set this FlowDocument to the Document property of the RichTextBox control to display the content.

    //public FlowDocument FlowDocument
    //{
    //    get
    //    {
    //        return GenerateLicenseInfoDocument();
    //        #region Obsolete Code
    //        //FlowDocument flowDocument = new FlowDocument();

    //        //Paragraph paragraph1 = new Paragraph();
    //        //paragraph1.Inlines.Add(new Bold(new Run($"{SoftwareName} v{SoftwareVersion}")));
    //        //flowDocument.Blocks.Add(paragraph1);

    //        //Paragraph paragraph2 = new Paragraph();
    //        //// 版权所有
    //        //paragraph2.Inlines.Add(new Run($"{Properties.Language.Resource.CopyrightOwnerText} © {CopyrightOwner} {ReleaseDate}"));
    //        //flowDocument.Blocks.Add(paragraph2);

    //        //Paragraph paragraph3 = new Paragraph();
    //        //// 本软件使用了以下开源库
    //        //paragraph3.Inlines.Add(new Run($"{Properties.Language.Resource.UsedLibraries}："));
    //        //flowDocument.Blocks.Add(paragraph3);

    //        //List openSourceList = new List();
    //        //foreach( Models.OpenSourceProjectInfos library in OpenSourceLibraries )
    //        //{
    //        //    Hyperlink hyperlink = new Hyperlink(new Run($"{library.Name} v{library.Version}"));
    //        //    hyperlink.NavigateUri = new Uri(library.Url); // set the hyperlink target
    //        //    openSourceList.ListItems.Add(new ListItem(new Paragraph(hyperlink)));
    //        //    // 许可证: 
    //        //    openSourceList.ListItems.Last().Blocks.Add(new Paragraph(new Italic(new Run($"{Properties.Language.Resource.LicenseText}{library.License}"))));
    //        //}
    //        //flowDocument.Blocks.Add(openSourceList);

    //        //Paragraph paragraph4 = new Paragraph();
    //        //paragraph4.Inlines.Add(new Run($"{Properties.Language.Resource.UsedThirdPartCompentsText}"));
    //        //flowDocument.Blocks.Add(paragraph4);

    //        //List thirdPartyList = new List();
    //        //foreach( Models.OpenSourceProjectInfos component in ThirdPartyComponents )
    //        //{
    //        //    // 许可证: 
    //        //    thirdPartyList.ListItems.Add(new ListItem(new Paragraph(new Bold(new Run($"{component.Name} v{component.Version}")))));
    //        //    thirdPartyList.ListItems.Last().Blocks.Add(new Paragraph(new Italic(new Run($"{Properties.Language.Resource.LicenseText}{component.License}"))));
    //        //}
    //        //flowDocument.Blocks.Add(thirdPartyList);

    //        //Paragraph paragraph5 = new Paragraph();
    //        //// 特别鸣谢：
    //        //paragraph5.Inlines.Add(new Run($"{Properties.Language.Resource.SpecialThanksText}"));
    //        //flowDocument.Blocks.Add(paragraph5);

    //        //List specialThanksList = new List();
    //        //foreach( string thanks in SpecialThanks )
    //        //{
    //        //    specialThanksList.ListItems.Add(new ListItem(new Paragraph(new Run(thanks))));
    //        //}
    //        //flowDocument.Blocks.Add(specialThanksList);

    //        //return flowDocument;
    //        #endregion
    //    }
    //}
    #endregion Some great infos

    #endregion Rewrite to AboutContentRichTextBox_Loaded()

    public FlowDocument GenerateLicenseInfoDocument( )
    {
        FlowDocument flowDocument = new FlowDocument();
        AddParagraph(flowDocument , new Bold(new Run($"{SoftwareName} v{SoftwareVersion}")));
        AddParagraph(flowDocument , new Run($"{Properties.Language.Resource.CopyrightOwnerText} © {CopyrightOwner} {ReleaseDate}"));
        AddParagraph(flowDocument , new Run($"{Properties.Language.Resource.UsedLibraries}"));
        AddList(flowDocument , OpenSourceLibraries);
        AddParagraph(flowDocument , new Run($"{Properties.Language.Resource.UsedThirdPartCompentsText}"));
        AddList(flowDocument , ThirdPartyComponents);
        AddParagraph(flowDocument , new Run($"{Properties.Language.Resource.SpecialThanksText}"));
        AddList(flowDocument , SpecialThanks);
        AddExtraInfosList(flowDocument , null);

        return flowDocument;
    }

    private void AddParagraph(FlowDocument flowDocument , Inline inline)
    {
        Paragraph paragraph = new Paragraph();
        paragraph.Inlines.Add(inline);
        flowDocument.Blocks.Add(paragraph);
    }

    private void AddList(FlowDocument flowDocument , List<Models.OpenSourceProjectInfos> items)
    {
        List list = new List();
        foreach( Models.OpenSourceProjectInfos item in items )
        {
            ListItem listItem = new ListItem();

            if( !string.IsNullOrEmpty(item.Url) )
            {
                Hyperlink hyperlink = new Hyperlink(new Run($"{item.Name} " + (item.Version.IsNullOrWhiteSpaceOrEmpty() ? "" : $"v{item.Version}")));
                hyperlink.NavigateUri = new Uri(item.Url); // set the hyperlink target
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate; // add event handler
                listItem.Blocks.Add(new Paragraph(hyperlink));
            }
            else
            {
                listItem.Blocks.Add(new Paragraph(new Run($"{item.Name} " + (item.Version.IsNullOrWhiteSpaceOrEmpty() ? "" : $"v{item.Version}"))));
            }

            if( !string.IsNullOrEmpty(item.License) )
            {
                listItem.Blocks.Add(new Paragraph(new Italic(new Run($"{Properties.Language.Resource.LicenseText} {item.License}"))));
            }

            list.ListItems.Add(listItem);
        }
        flowDocument.Blocks.Add(list);
    }

    private void Hyperlink_RequestNavigate(object sender , RequestNavigateEventArgs e)
    {
        try
        {
            // open the link in the default browser
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri ,
                UseShellExecute = true
            };

            // open the link in Chrome browser
            //ProcessStartInfo startInfo = new ProcessStartInfo
            //{
            //    FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" ,
            //    Arguments = e.Uri.AbsoluteUri ,
            //    UseShellExecute = false
            //};

            Process.Start(startInfo);
        }
        catch( Exception ex )
        {
            // handle the exception
        }

        e.Handled = true;
    }

    //TODO: Add update/usage/etc infos
    private void AddExtraInfosList(FlowDocument flowDocument , List<Models.OpenSourceProjectInfos> items)
    {
        AddParagraph(flowDocument , new Run($"{Properties.Language.Resource.AboutSlogan}"));
    }

    #region Get OpenSourceProject Infos Methods
    private void AddNuGetPackageInfoToList(string packageName)
    {
        // retrieve assembly information for the package
        Assembly assembly = Assembly.Load(packageName);
        AssemblyName assemblyName = assembly.GetName();

        // retrieve package information from the assembly metadata
        string name = assemblyName.Name;
        string version = assemblyName.Version.ToString();
        string license = null;
        string url = null;

        object[] attributes = assembly.GetCustomAttributes(false);
        foreach( object attribute in attributes )
        {
            if( attribute is AssemblyCompanyAttribute companyAttribute )
            {
                url = companyAttribute.Company;
            }
            else if( attribute is AssemblyMetadataAttribute metadataAttribute )
            {
                if( metadataAttribute.Key == "LicenseUrl" )
                {
                    license = metadataAttribute.Value;
                }
                else if( metadataAttribute.Key == "ProjectUrl" )
                {
                    url = metadataAttribute.Value;
                }
            }
        }

        // if package information is found, add it to the list
        if( name != null && version != null )
        {
            Models.OpenSourceProjectInfos packageInfo = new Models.OpenSourceProjectInfos
            {
                Name = name ,
                Version = version ,
                License = license ,
                Url = url
            };
            _openSourceLibraries.Add(packageInfo);
        }
    }

    //public List<Models.OpenSourceProjectInfos> GetPackageReferencesFromCsproj(string csprojFilePath)
    //{
    //    List<Models.OpenSourceProjectInfos> packageReferences = new List<Models.OpenSourceProjectInfos>();

    //    // load the csproj file
    //    ProjectRootElement root = ProjectRootElement.Open(csprojFilePath);

    //    // retrieve all PackageReference elements
    //    foreach( ProjectItemElement item in root.Items )
    //    {
    //        if( item.ItemType == "PackageReference" )
    //        {
    //            string name = item.Include;
    //            string version = item.GetMetadataValue("Version");
    //            string license = item.GetMetadataValue("LicenseUrl");
    //            string url = item.GetMetadataValue("ProjectUrl");

    //            // if package information is found, add it to the list
    //            if( name != null && version != null )
    //            {
    //                Models.OpenSourceProjectInfos packageInfo = new Models.OpenSourceProjectInfos
    //                {
    //                    Name = name ,
    //                    Version = version ,
    //                    License = license ,
    //                    Url = url
    //                };
    //                packageReferences.Add(packageInfo);
    //            }
    //        }
    //    }

    //    return packageReferences;
    //}

    //private void AddNuGetPackageInfoToList(string packageName)
    //{
    //    // retrieve NuGet package information
    //    IPackageMetadata packageMetadata = PackageRepositoryFactory.Default
    //        .CreateRepository("https://api.nuget.org/v3/index.json")
    //        .FindPackage(packageName)
    //        ?.Metadata;

    //    // if package is found, add its information to the list
    //    if( packageMetadata != null )
    //    {
    //        Models.OpenSourceProjectInfos packageInfo = new Models.OpenSourceProjectInfos
    //        {
    //            Name = packageMetadata.Id ,
    //            Version = packageMetadata.Version.ToString() ,
    //            License = packageMetadata.LicenseUrl?.ToString() ,
    //            Url = packageMetadata.ProjectUrl?.ToString()
    //        };
    //        _openSourceLibraries.Add(packageInfo);
    //    }
    //}

    #endregion Get OpenSourceProject Infos Methods

    #endregion FlowDocumentContent



}
