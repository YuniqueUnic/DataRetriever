using MiniExcelLibs;
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

        try
        {
            await MiniExcelLibs.MiniExcel.SaveAsAsync(_FullPath , TargetObj ,
                sheetName: _sheetName ?? "Sheet1" , excelType: _excelType ?? MiniExcelLibs.ExcelType.XLSX);
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

}


