using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace VSTSDataProvider.Common;


public class ExcelOperator
{

    private string _filePath;
    private string _fileName;
    private string _FullPath => EnsureDefaultFields();

    private string? _sheetName;
    private MiniExcelLibs.ExcelType? _excelType;

    public ExcelOperator(string filePath)
    {
        _filePath = filePath;
    }

    public ExcelOperator(string fileName , string filePath)
    {
        _filePath = filePath;
    }

    public ExcelOperator SetSheetName(string sheetName)
    {
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
        if( string.IsNullOrEmpty(_filePath) || !Directory.Exists(_filePath) )
        {
            _filePath = Environment.CurrentDirectory;
        }

        if( string.IsNullOrEmpty(_fileName) )
        {
            _fileName = $"Exported {Guid.NewGuid()}";

            while( File.Exists(Path.Combine(_filePath + _fileName)) )
            {
                _fileName = $"Exported {Guid.NewGuid()}";
            }
        }

        if( !_excelType.HasValue )
        {
            _excelType = MiniExcelLibs.ExcelType.XLSX;
        }

        return Path.Combine(_filePath , _fileName + "." + _excelType.ToString());
    }

    public async Task<bool> Export<TObject>(ConcurrentBag<TObject> TargetObj)
    where TObject : class, Models.IResultsModel
    {
        if( TargetObj == null ) { return false; }

        try
        {
            await MiniExcelLibs.MiniExcel.SaveAsAsync(_FullPath , TargetObj ,
                sheetName: _sheetName ?? "TestCases" , excelType: _excelType ?? MiniExcelLibs.ExcelType.XLSX);
        }
        catch( Exception )
        {
            return false;
            throw;
        }

        return true;
    }

    public async Task<bool> ExportAs<TObject>(TObject TargetObj)
    {
        if( TargetObj == null ) { return false; }

        try
        {
            await MiniExcelLibs.MiniExcel.SaveAsAsync(_FullPath , TargetObj ,
                sheetName: _sheetName ?? "TestCases" , excelType: _excelType ?? MiniExcelLibs.ExcelType.XLSX);
        }
        catch( Exception )
        {
            return false;
            throw;
        }

        return true;
    }
}


public class ExcelOperatorHelper : IDataReader
{
    private bool disposedValue;

    public object this[int i] => throw new NotImplementedException();

    public object this[string name] => throw new NotImplementedException();

    public int Depth => throw new NotImplementedException();

    public bool IsClosed => throw new NotImplementedException();

    public int RecordsAffected => throw new NotImplementedException();

    public int FieldCount => throw new NotImplementedException();

    public void Close( )
    {
        throw new NotImplementedException();
    }

    public bool GetBoolean(int i)
    {
        throw new NotImplementedException();
    }

    public byte GetByte(int i)
    {
        throw new NotImplementedException();
    }

    public long GetBytes(int i , long fieldOffset , byte[]? buffer , int bufferoffset , int length)
    {
        throw new NotImplementedException();
    }

    public char GetChar(int i)
    {
        throw new NotImplementedException();
    }

    public long GetChars(int i , long fieldoffset , char[]? buffer , int bufferoffset , int length)
    {
        throw new NotImplementedException();
    }

    public IDataReader GetData(int i)
    {
        throw new NotImplementedException();
    }

    public string GetDataTypeName(int i)
    {
        throw new NotImplementedException();
    }

    public DateTime GetDateTime(int i)
    {
        throw new NotImplementedException();
    }

    public decimal GetDecimal(int i)
    {
        throw new NotImplementedException();
    }

    public double GetDouble(int i)
    {
        throw new NotImplementedException();
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type GetFieldType(int i)
    {
        throw new NotImplementedException();
    }

    public float GetFloat(int i)
    {
        throw new NotImplementedException();
    }

    public Guid GetGuid(int i)
    {
        throw new NotImplementedException();
    }

    public short GetInt16(int i)
    {
        throw new NotImplementedException();
    }

    public int GetInt32(int i)
    {
        throw new NotImplementedException();
    }

    public long GetInt64(int i)
    {
        throw new NotImplementedException();
    }

    public string GetName(int i)
    {
        throw new NotImplementedException();
    }

    public int GetOrdinal(string name)
    {
        throw new NotImplementedException();
    }

    public DataTable? GetSchemaTable( )
    {
        throw new NotImplementedException();
    }

    public string GetString(int i)
    {
        throw new NotImplementedException();
    }

    public object GetValue(int i)
    {
        throw new NotImplementedException();
    }

    public int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public bool IsDBNull(int i)
    {
        throw new NotImplementedException();
    }

    public bool NextResult( )
    {
        throw new NotImplementedException();
    }

    public bool Read( )
    {
        throw new NotImplementedException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if( !disposedValue )
        {
            if( disposing )
            {
                // TODO: 释放托管状态(托管对象)
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~ExcelOperatorHelper()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose( )
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}