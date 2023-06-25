using MiniExcelLibs;
using MiniExcelLibs.Csv;
using MiniExcelLibs.OpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VSTSDataProvider.Common.Helpers;
using VSTSDataProvider.Models;

namespace VSTSDataProvider.Common;

public record class ExcelOperatorResult
{
    public bool SucceedDone;
    public string? FullPath;
    public string? Info;
    public IEnumerable<IResultsModel>? resultModels;
}

public record class ExcelModifyRule
{
    public string ColumnName;
    public Func<object , object> ModifyRule;
}


public class ExcelOperator
{
    public static ExcelType ParseExcelType(string fileNameWithExtension)
    {
        return Path.GetExtension(fileNameWithExtension) switch
        {
            ".xlsx" => MiniExcelLibs.ExcelType.XLSX,
            ".csv" => MiniExcelLibs.ExcelType.CSV,
            _ => MiniExcelLibs.ExcelType.UNKNOWN
        };
    }

    private string _directoryPath;
    private string _fileName;
    private string _FullPath => EnsureDefaultFields();

    private string? _sheetName;
    private MiniExcelLibs.ExcelType? _excelType;

    public ExcelOperator(string directoryPath) => _directoryPath = directoryPath;

    public ExcelOperator(string fileName , string directoryPath)
    {
        _fileName = fileName;
        _directoryPath = directoryPath;
    }

    public ExcelOperator setFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public ExcelOperator SetSheetName(string sheetName)
    {
        if( sheetName.Length >= 30 )
        {
            sheetName.Substring(0 , 30);
        }

        _sheetName = sheetName;
        return this;
    }

    public ExcelOperator SetExcelType(MiniExcelLibs.ExcelType excelType)
    {
        _excelType = excelType;
        return this;
    }

    private string EnsureDefaultFields( )
    {
        if( string.IsNullOrEmpty(_directoryPath) || !Directory.Exists(_directoryPath) )
        {
            _directoryPath = Environment.CurrentDirectory;
        }

        if( string.IsNullOrEmpty(_fileName) )
        {
            _fileName = $"Exported {Guid.NewGuid()}";

            while( File.Exists(System.IO.Path.Combine(_directoryPath + _fileName)) )
            {
                _fileName = $"Exported {Guid.NewGuid()}";
            }
        }

        if( !_excelType.HasValue )
        {
            _excelType = MiniExcelLibs.ExcelType.XLSX;
        }

        return System.IO.Path.Combine(_directoryPath , _fileName);
    }

    public async Task<ExcelOperatorResult> ExportAsync<TObject>(TObject TargetObj)
    {
        var result = new ExcelOperatorResult()
        {
            SucceedDone = false ,
            FullPath = _FullPath ,
            Info = "NullReferenceException" ,
        };

        if( TargetObj == null ) { return result; }

        var miniExcelConfig = GetDefaultConfiguration(_excelType);

        try
        {
            await MiniExcelLibs.MiniExcel.SaveAsAsync(_FullPath ,
                TargetObj ,
                sheetName: _sheetName ?? "Sheet1" ,
                excelType: _excelType ?? MiniExcelLibs.ExcelType.XLSX ,
                configuration: miniExcelConfig ?? default).ConfigureAwait(false);

            result.SucceedDone = true;
        }
        catch( Exception e )
        {
            result.Info = e.Message;
            return result;
            throw;
        }

        return result;
    }

    public async Task<ExcelOperatorResult> ImportAsyncBy<TModel>(string filefullpath)
        where TModel : class, Models.IResultsModel, new()
    {
        var result = new ExcelOperatorResult()
        {
            SucceedDone = false ,
            FullPath = _FullPath ,
            Info = "NullReferenceException" ,
            resultModels = null ,
        };

        if( filefullpath.IsNullOrWhiteSpaceOrEmpty() ) return result;

        if( !File.Exists(filefullpath) )
        {
            result.Info = $"{filefullpath} not exists.";
            return result;
        }

        using( var stream = File.OpenRead(filefullpath) )
        {
            try
            {
                result.resultModels = (await stream.QueryAsync<TModel>(excelType: _excelType ?? ExcelType.XLSX)).ToList<TModel>();
                result.Info = $"Import data from {filefullpath} successfully.";
                result.SucceedDone = true;
            }
            catch( Exception e )
            {
                result.Info = e.Message;
                throw;
            }

            return result;
        }
    }

    public async Task<ExcelOperatorResult> ImportAsync<TModel>( )
       where TModel : class, Models.IResultsModel, new()
    {
        return await ImportAsyncBy<TModel>(_FullPath);
    }

    /// <summary>
    /// Modify column values according to rules.
    /// Automatically determine whether the column exists.
    /// Only existing columns will be modified.
    /// </summary>
    /// <param name="fileFullPath"></param>
    /// <param name="excelModifyRules"></param>
    /// <returns></returns>
    public async static Task<ExcelOperatorResult> ModifyColumn(string fileFullPath , ExcelModifyRule[] excelModifyRules)
    {
        var result = new ExcelOperatorResult()
        {
            SucceedDone = false ,
            FullPath = fileFullPath ,
            Info = "NullReferenceException" ,
        };

        if( excelModifyRules is null )
        {
            result.Info = $"{excelModifyRules} is null.";
            return result;
        }

        if( !File.Exists(fileFullPath) )
        {
            result.Info = $"{fileFullPath} not exists.";
            return result;
        }

        ExcelType _excelType = ParseExcelType(Path.GetFileName(fileFullPath));

        var data = (await MiniExcelLibs.MiniExcel.QueryAsync(fileFullPath , useHeaderRow: true , excelType: _excelType))
                                                   .Cast<IDictionary<string , object>>();

        List<Dictionary<string , object>> listDics = data.Select(dict =>
                                                          dict.ToDictionary(kv =>
                                                               kv.Key , kv => kv.Value))
                                                               .ToList();

        result.Info = string.Empty;

        foreach( var item in listDics )
        {
            for( int i = 0; i < excelModifyRules.Length; i++ )
            {
                if( item.ContainsKey(excelModifyRules[i].ColumnName) )
                {
                    // modify the value of the column by modifyRule ★☆★☆★
                    item[excelModifyRules[i].ColumnName] = excelModifyRules[i].ModifyRule(item[excelModifyRules[i].ColumnName]);
                    result.Info += $"{excelModifyRules[i].ColumnName} modified successfully." + Environment.NewLine;
                }
                else
                {
                    result.Info += $"{excelModifyRules[i].ColumnName} not exists." + Environment.NewLine;
                }
            }
        }

        try
        {
            dynamic miniExcelConfig = GetDefaultConfiguration(_excelType);

            // Use the await Task.Yield() statement before SaveAsAsync the method to release the current thread and
            // perform the save operation on the background thread.
            // This will cause the save operation to be performed on the background thread and allow the main thread to continue processing other tasks,
            // thereby improving the responsiveness of the application.
            await Task.Yield();

            await MiniExcelLibs.MiniExcel.SaveAsAsync(fileFullPath ,
                                                      listDics ,
                                                      excelType: _excelType ,
                                                      configuration: miniExcelConfig ?? default ,
                                                      overwriteFile: true).ConfigureAwait(false);

            result.SucceedDone = true;
        }
        catch( Exception e )
        {
            result.Info += e.Message + Environment.NewLine;
            return result;
            throw;
        }

        return result;
    }

    private static dynamic GetDefaultConfiguration(ExcelType? _excelType)
    {
        dynamic miniExcelConfig = new OpenXmlConfiguration();

        if( _excelType.Equals(ExcelType.XLSX) )
        {
            miniExcelConfig = new OpenXmlConfiguration()
            {
                TableStyles = TableStyles.None ,
            };
        }
        else if( _excelType.Equals(ExcelType.CSV) )
        {
            miniExcelConfig = new CsvConfiguration();
        }

        return miniExcelConfig;
    }
}


