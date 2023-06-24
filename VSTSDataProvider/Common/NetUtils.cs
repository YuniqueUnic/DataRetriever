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

    //需要再完善
    public static async Task<string> SendRequestWithAccessToken(string apiUrl , string accessToken , Action callBackAction = null)
    {
        using( var httpClient = new HttpClient() )
        {
            if( !accessToken.IsNullOrWhiteSpaceOrEmpty() )
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic" , Convert.ToBase64String
                (System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}" , "" , accessToken))));

            }

            using( var response = await httpClient.GetAsync(apiUrl) )
            {
                //    string responseData = await response.Content.ReadAsStringAsync();

                //// 发送 HTTP GET 请求并获取响应
                //HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                // 创建一个 ResponseHandler 对象，以处理响应并执行回调函数
                ResponseHandler responseHandler = new ResponseHandler(response , callBackAction);

                // 调用 ResponseHandler 对象的 HandleResponseAsync 方法，以处理响应并返回 JObject 对象
                return await responseHandler.HandleResponseAsync<string>();

            }
        }

    }

    /// <summary>
    /// 使用指定的 Cookie 值向指定的 API 地址发送 HTTP GET 请求，并在请求完成后执行指定的回调函数。
    /// </summary>
    /// <param name="apiUrl">要发送请求的 API 地址。</param>
    /// <param name="accessCookie">要在请求头中设置的 Cookie 值。</param>
    /// <param name="callBackAction">请求完成后要执行的回调函数。</param>
    /// <returns>表示操作结果的 Task，其 Result 属性将包含响应内容的 JObject 对象。</returns>
    public static async Task<string> SendRequestWithCookieForStr(string apiUrl , string accessCookie , Action callBackAction = null)
    {
        // 创建一个 HttpClient 对象，并使用 using 语句确保在使用完毕后正确释放资源
        using( HttpClient httpClient = new HttpClient() )
        {
            // 如果 accessCookie 不为空，将其添加到请求头中的 Cookie 值中
            if( !string.IsNullOrEmpty(accessCookie) )
            {
                httpClient.DefaultRequestHeaders.Add("Cookie" , accessCookie);
            }

            // 发送 HTTP GET 请求并获取响应
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            // 创建一个 ResponseHandler 对象，以处理响应并执行回调函数
            ResponseHandler responseHandler = new ResponseHandler(response , callBackAction);

            // 调用 ResponseHandler 对象的 HandleResponseAsync 方法，以处理响应并返回 JObject 对象
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
    public static async Task<JObject> SendRequestWithCookieForJObj(string apiUrl , string accessCookie , Action callBackAction = null)
    {
        // 创建一个 HttpClient 对象，并使用 using 语句确保在使用完毕后正确释放资源
        using( HttpClient httpClient = new HttpClient() )
        {
            // 如果 accessCookie 不为空，将其添加到请求头中的 Cookie 值中
            if( !string.IsNullOrEmpty(accessCookie) )
            {
                httpClient.DefaultRequestHeaders.Add("Cookie" , accessCookie);
            }

            // 发送 HTTP GET 请求并获取响应
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            // 创建一个 ResponseHandler 对象，以处理响应并执行回调函数
            ResponseHandler responseHandler = new ResponseHandler(response , callBackAction);

            // 调用 ResponseHandler 对象的 HandleResponseAsync 方法，以处理响应并返回 JObject 对象
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