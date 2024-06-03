using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using VSTSDataProvider.Common.Helpers;

namespace VSTSDataProvider.Common;


public class NetUtils
{
    private static NetUtils _instance;
    private static readonly object _lockObject = new object();
    private HttpClient _httpClient;

    private NetUtils( )
    {
        _httpClient = new HttpClient();
    }

    public static NetUtils Instance
    {
        get
        {
            lock( _lockObject )
            {
                if( _instance == null )
                {
                    _instance = new NetUtils();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 使用指定的 Token 值向指定的 API 地址发送 HTTP GET 请求，并在请求完成后执行指定的回调函数。
    /// </summary>
    /// <param name="apiUrl">要发送请求的 API 地址。</param>
    /// <param name="accessToken">要在请求头中设置的 Token 值。</param>
    /// <param name="callBackAction">请求完成后要执行的回调函数。</param>
    /// <returns>表示操作结果的 Task，其 Result 属性将包含响应内容的 JObject 对象。</returns>
    public async Task<string> SendRequestWithAccessTokenStr(string apiUrl , string accessToken , Action callBackAction = null)
    {
        if( !_httpClient.DefaultRequestHeaders.Contains("Authorization") && !accessToken.IsNullOrWhiteSpaceOrEmpty() )
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic" , Convert.ToBase64String
            (System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}" , "" , accessToken))));
        }

        using( var response = await _httpClient.GetAsync(apiUrl) )
        {
            ResponseHandler responseHandler = new ResponseHandler(response , callBackAction);
            return await responseHandler.HandleResponseAsync<string>();
        }
    }

    /// <summary>
    /// 使用指定的 Cookie 值向指定的 API 地址发送 HTTP GET 请求，并在请求完成后执行指定的回调函数。
    /// </summary>
    /// <param name="apiUrl">要发送请求的 API 地址。</param>
    /// <param name="accessCookie">要在请求头中设置的 Cookie 值。</param>
    /// <param name="callBackAction">请求完成后要执行的回调函数。</param>
    /// <returns>表示操作结果的 Task，其 Result 属性将包含响应内容的 JObject 对象。</returns>
    public async Task<string> SendRequestWithCookieForStr(string apiUrl , string accessCookie , Action callBackAction = null)
    {
        if( !_httpClient.DefaultRequestHeaders.Contains("Cookie") && !accessCookie.IsNullOrWhiteSpaceOrEmpty() )
        {
            _httpClient.DefaultRequestHeaders.Add("Cookie" , accessCookie);
        }

        using( var response = await _httpClient.GetAsync(apiUrl) )
        {
            // 创建一个 ResponseHandler 对象，以处理响应并执行回调函数
            ResponseHandler responseHandler = new ResponseHandler(response , callBackAction);

            // 调用 ResponseHandler 对象的 HandleResponseAsync 方法
            return await responseHandler.HandleResponseAsync<string>();
        }
    }

    public async Task<JObject> SendRequestWithCookieForJObj(string apiUrl , string accessCookie , Action callBackAction = null)
    {
        if( !_httpClient.DefaultRequestHeaders.Contains("Cookie") )
        {
            _httpClient.DefaultRequestHeaders.Add("Cookie" , accessCookie);
        }

        using( var response = await _httpClient.GetAsync(apiUrl) )
        {
            ResponseHandler responseHandler = new ResponseHandler(response , callBackAction);
            return await responseHandler.HandleResponseAsync<JObject>();
        }
    }
}


// Refractor --- HttpMethods
public class ResponseHandler
{
    private readonly HttpResponseMessage _response;
    private readonly Action _callBackAction;

    public ResponseHandler(HttpResponseMessage response , Action callBackAction)
    {
        _response = response;
        _callBackAction = callBackAction;
    }

    public async Task<T> HandleResponseAsync<T>( )
    {
        if( _response.IsSuccessStatusCode )
        {
            return await ParseResponseAsync<T>();
        }

        return await HandleErrorResponseAsync<T>();
    }

    private async Task<T> ParseResponseAsync<T>( )
    {
        var responseContent = await _response.Content.ReadAsStringAsync();

        T result = typeof(T) switch
        {
            Type t when t == typeof(string) => (T)(object)responseContent,
            _ => JsonConvert.DeserializeObject<T>(responseContent),
        };

        if( _callBackAction is not null )
        {
            _callBackAction();
        }
        return result;
    }

    private async Task<T> HandleErrorResponseAsync<T>( )
    {
        var errorMessage = $"{(int)_response.StatusCode} {_response.ReasonPhrase}\n\nClick OK Button to Retry...\n\n{_response}";

        var result = MessageBox.Show(errorMessage , "请求失败" , MessageBoxButton.OKCancel , MessageBoxImage.Warning);

        if( result == MessageBoxResult.OK )
        {
            return await RetryAsync<T>();
        }
        if( _callBackAction is not null )
        {
            _callBackAction();
        }
        return default;
    }

    private async Task<T> RetryAsync<T>( )
    {
        var response = await _response.RequestMessage.GetHttpClient().GetAsync(_response.RequestMessage.RequestUri);
        var responseHandler = new ResponseHandler(response , _callBackAction);
        return await responseHandler.HandleResponseAsync<T>();
    }
}

public static class HttpClientExtensions
{
    public static HttpClient GetHttpClient(this HttpRequestMessage requestMessage)
    {
        return new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            DefaultRequestHeaders =
            {
                { "Cookie", requestMessage.Headers.GetValues("Cookie") }
            }
        };
    }
}