using AntDesign;
using Caviar.Models.SystemData;
using Caviar.AntDesignPages.Helper;
using Caviar.AntDesignPages.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caviar.Demo.Models;
/// <summary>
/// 生成者：未登录用户
/// 生成时间：2021/5/14 15:46:09
/// 代码由代码生成器自动生成，更改的代码可能被进行替换
/// 可在上层目录使用partial关键字进行扩展
/// </summary>
namespace Caviar.Demo.AntDesignUI
{
    public partial class Index
    {
        #region 属性注入
        /// <summary>
        /// HttpClient
        /// </summary>
        [Inject]
        HttpHelper Http { get; set; }
        /// <summary>
        /// 全局提示
        /// </summary>
        [Inject]
        MessageService Message { get; set; }
        /// <summary>
        /// 用户配置
        /// </summary>
        [Inject]
        UserConfigHelper UserConfig { get; set; }
        /// <summary>
        /// 导航管理器
        /// </summary>
        [Inject]
        NavigationManager NavigationManager { get; set; }
        /// <summary>
        /// 确认框
        /// </summary>
        [Inject]
        ConfirmService Confirm { get; set; }
        #endregion

        #region 属性
        /// <summary>
        /// 数据源
        /// </summary>
        List<ViewSysPowerMenu> DataSource { get; set; }
        /// <summary>
        /// 总计
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 页数
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 页面大小
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 表头
        /// </summary>
        List<ViewModelHeader> ViewModelHeaders { get; set; }
        /// <summary>
        /// 按钮
        /// </summary>
        List<ViewMenu> Buttons { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <returns></returns>
        async Task GetViewSysPowerMenu()
        {
            var result = await Http.GetJson<PageData<ViewSysPowerMenu>>("SysPowerMenu/GetPages");
            if (result.Status != 200) return;
            if (result.Data != null)
            {
                DataSource = result.Data.Rows;
                Total = result.Data.Total;
                PageIndex = result.Data.PageIndex;
                PageSize = result.Data.PageSize;
            }
        }
        /// <summary>
        /// 获取按钮
        /// </summary>
        /// <returns></returns>
        async Task<List<ViewMenu>> GetPowerButtons()
        {
            string url = NavigationManager.Uri.Replace(NavigationManager.BaseUri, "");
            var result = await Http.GetJson<List<ViewMenu>>("Menu/GetButtons?url=" + url);
            if (result.Status != 200) return new List<ViewMenu>();
            return result.Data;
        }
        /// <summary>
        /// 获取表头
        /// </summary>
        /// <returns></returns>
        async Task<List<ViewModelHeader>> GetHeader()
        {
            var modelNameList = await Http.GetJson<List<ViewModelHeader>>("Base/GetModelHeader?name=SysPowerMenu");
            if (modelNameList.Status == 200)
            {
                var item = modelNameList.Data.SingleOrDefault(u => u.TypeName.ToLower() == "icon");
                if (item != null) item.ModelType = "icon";
                return modelNameList.Data;
            }
            return new List<ViewModelHeader>();
        }
        #endregion

        #region 回调
        void RowCallback(RowCallbackData<ViewSysPowerMenu> row)
        {
            switch (row.Menu.MenuName)
            {
                default:
                    break;
            }
        }

        async void HandleOk(ViewMenu e)
        {
            switch (e.MenuName)
            {
                default:
                    break;
            }

        }
        #endregion

        #region 重写
        protected override async Task OnInitializedAsync()
        {
            GetViewSysPowerMenu();//获取数据源
            ViewModelHeaders = await GetHeader();//获取表头
            Buttons = await GetPowerButtons();//获取按钮
        }
        #endregion

    }
}
