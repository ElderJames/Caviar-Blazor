using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System;
using AntDesign;
using System.Collections.Generic;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Http;
using Caviar.SharedKernel;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace Caviar.AntDesignUI.Helper
{
    public partial class HttpHelper
    {
        NotificationService _notificationService;
        NavigationManager _navigationManager;
        MessageService _message;
        IJSRuntime _jSRuntime;
        ILocalStorageService _localStorageService;
        public string TokenName => CurrencyConstant.Authorization;
        public HttpHelper(HttpClient http, 
            NotificationService _notice,
            NavigationManager navigationManager, 
            MessageService message,
            IJSRuntime JsRuntime,
            ILocalStorageService localStorageService,
            UserConfig userConfig)
        {
            HttpClient = http;
            _notificationService = _notice;
            _navigationManager = navigationManager;
            _message = message;
            _jSRuntime = JsRuntime;
            _localStorageService = localStorageService;
            LanguageService_LanguageChanged(null, userConfig.LanguageService.CurrentCulture);
            userConfig.LanguageService.LanguageChanged += LanguageService_LanguageChanged;
        }

        private void LanguageService_LanguageChanged(object sender, System.Globalization.CultureInfo e)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(CurrencyConstant.LanguageHeader))
            {
                HttpClient.DefaultRequestHeaders.Remove(CurrencyConstant.LanguageHeader);
            }
            HttpClient.DefaultRequestHeaders.Add(CurrencyConstant.LanguageHeader, e.Name);
        }


        public HttpClient HttpClient { get; }
        public async Task<ResultMsg<T>> GetJson<T>(string address)
        {
            var result = await HttpRequest<T,T>(address,"get",default);
            return result;
        }

        public async Task<ResultMsg> GetJson(string address)
        {
            var result = await HttpRequest<object,object>(address, "get", default);
            return result;
        }

        public async Task<ResultMsg<T>> PostJson<K, T>(string address,K data)
        {
            var result = await HttpRequest<K,T>(address, "post", data);
            return result;
        }

        public async Task<ResultMsg> PostJson<K>(string address, K data)
        {
            var result = await HttpRequest<K, object>(address, "post", data);
            return result;
        }

        async Task<ResultMsg<T>> HttpRequest<K,T>(string address,string model, K data)
        {
            //var token = await _localStorageService.GetItemAsync<string>(Config.TokenName);
            //HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            ResultMsg<T> result;
            try
            {
                HttpResponseMessage responseMessage;
                if (model.ToLower() == "get")
                {
                    responseMessage = await HttpClient.GetAsync(address);
                }
                else if(model.ToLower() == "post")
                {
                    responseMessage = await HttpClient.PostAsJsonAsync(address, data);
                }
                else
                {
                    throw new Exception("暂不支持的请求方法");
                }
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //当这里解析失败，又找不到具体原因，试试使用Newtonsoft.json进行解析
                    var resultMsg = await responseMessage.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<ResultMsg<T>>(resultMsg);
                }
                else
                {
                    result = new ResultMsg<T>()
                    {
                        Title = "请求失败:" + responseMessage.ReasonPhrase,
                        Status = (int)responseMessage.StatusCode,
                    };
                }
            }
            catch(Exception e)
            {
                result = new ResultMsg<T>()
                {
                    Title = "请求失败，发生请求错误",
                    Detail = e.Message,
                    Status = 500,
                };
            }
            Response(result);
            return result;
        }
        
        public async void Response(ResultMsg result)
        {
            switch (result.Status)
            {
                case StatusCodes.Status200OK://正确响应
                    break;
                case StatusCodes.Status307TemporaryRedirect://重定向专用
                    _navigationManager.NavigateTo(result.Title);
                    break;
                case StatusCodes.Status401Unauthorized://退出登录
                    HttpClient.DefaultRequestHeaders.Authorization = null;
                    break;
                case StatusCodes.Status404NotFound:
                case StatusCodes.Status400BadRequest:
                case StatusCodes.Status500InternalServerError://发生严重错误
                default:
                    string msg = "";
                    if (!string.IsNullOrEmpty(result.Detail))
                    {
                        msg += "错误详细信息：" + result.Detail + "<br>";
                    }
                    if (!string.IsNullOrEmpty(result.Url))
                    {
                        msg += $"<a target='_Blank' href='{result.Url}'>点击查看解决办法</a><br>";
                    }
                    await _notificationService.Open(new NotificationConfig()
                    {
                        Message = result.Title,
                        Description = msg,
                        NotificationType = NotificationType.Error
                    });
                    break;
            }
        }


    }
}