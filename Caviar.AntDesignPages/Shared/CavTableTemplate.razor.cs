﻿using AntDesign;
using Caviar.Models.SystemData;
using Caviar.AntDesignPages.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Web;

namespace Caviar.AntDesignPages.Shared
{
    public partial class CavTableTemplate<TData>
    {
        [Parameter]
        public List<TData> DataSource { get; set; }
        [Parameter]
        public int Total { get; set; }
        [Parameter]
        public int PageIndex { get; set; }
        [Parameter]
        public int PageSize { get; set; }

        [Parameter]
        public List<ViewMenu> Buttons { get; set; }

        [Parameter]
        public string ModelName { get; set; }
        [Parameter]
        public Func<TData, IEnumerable<TData>> TreeChildren { get; set; } = _ => Enumerable.Empty<TData>();
        [Parameter]
        public List<ViewModelFields> ViewModelFields { get; set; }
        [Inject]
        HttpHelper Http { get; set; }
        [Parameter]
        public EventCallback<PaginationEventArgs> PageIndexChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Query.QueryObj = typeof(TData).Name;

            if (!string.IsNullOrEmpty(ModelName))
            {
                var modelNameList = await Http.GetJson<List<ViewModelFields>>("Permission/GetFields?modelName=" + ModelName);
                if (modelNameList.Status == 200)
                {
                    ViewModelFields = modelNameList.Data;
                }
            }
            
        }

        [Parameter]
        public EventCallback<RowCallbackData<TData>> RowCallback { get; set; }

        async void RoleAction(RowCallbackData<TData> data)
        {
            if (RowCallback.HasDelegate)
            {
                await RowCallback.InvokeAsync(data);
            }
            
        }
        [Inject]
        IJSRuntime JSRuntime { get; set; }
        [Inject]
        NavigationManager Navigation { get; set; }
        [Inject]
        CavModal CavModal { get; set; }
        RowCallbackData<TData> CurrRow { get; set; }
        async void ButtonClick(ViewMenu menu, TData data)
        {
            CurrRow = new RowCallbackData<TData>()
            {
                Menu = menu,
                Data = data,
            };
            switch (menu.TargetType)
            {
                case TargetType.CurrentPage:
                    var parameter = "";
                    if (menu.ButtonPosition == ButtonPosition.Row)
                    {
                        parameter = $"?parameter={HttpUtility.UrlEncode(JsonSerializer.Serialize(CurrRow.Data))}"; 
                    }
                    Navigation.NavigateTo(menu.Url + parameter);
                    break;
                case TargetType.EjectPage:
                    List<KeyValuePair<string, object?>> paramenter = new List<KeyValuePair<string, object?>>();
                    if (menu.ButtonPosition == ButtonPosition.Row)
                    {
                        //因为引用类型，这里进行一次转换，相当于深度复制
                        //否则更改内容然后取消，列表会发生改变
                        CurrRow.Data.AToB(out TData dataSource);
                        paramenter.Add(new KeyValuePair<string, object?>("DataSource", dataSource));
                    }
                    paramenter.Add(new KeyValuePair<string, object?>("Url", menu.Url));
                    await CavModal.Create(menu.Url, menu.MenuName, HandleOk, paramenter);
                    break;
                case TargetType.NewLabel:
                    //await JSRuntime.InvokeVoidAsync("open", menu.Url, "_blank");
                    break;
                case TargetType.Callback:
                    RoleAction(CurrRow);
                    break;
                default:
                    break;
            }
        }

        public async void HandleOk()
        {
            RoleAction(CurrRow);
        }

        #region 查询条件
        IEnumerable<string> _selectedValues;
        [Parameter]
        public bool IsOpenQuery { get; set; } = true;


        ViewQuery Query = new ViewQuery();

        void OnRangeChange(DateRangeChangedEventArgs args)
        {
            Query.StartTime = args.Dates[0];
            Query.EndTime = args.Dates[1];
        }


        public async void FuzzyQuery()
        {
            Query.QueryField = _selectedValues?.ToList();
            var result = await Http.PostJson<ViewQuery, List<TData>>("CaviarBase/FuzzyQuery", Query);
            DataSource = result.Data;
            StateHasChanged();
        }
        #endregion
    }
}
