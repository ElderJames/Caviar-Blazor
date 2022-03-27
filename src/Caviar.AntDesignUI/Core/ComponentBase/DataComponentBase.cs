﻿using AntDesign;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Caviar.SharedKernel.Entities.View;
using Caviar.SharedKernel.Entities;
using System;

namespace Caviar.AntDesignUI.Core
{
    public partial class DataComponentBase<ViewT,T> : ComponentBase,ITableTemplate where ViewT:IView<T>,new() where T: IUseEntity,new()
    {
        #region 参数
        /// <summary>
        /// 则默认当前页面url
        /// </summary>
        [Parameter]
        public string CurrentUrl { get; set; }
        /// <summary>
        /// 数据源
        /// </summary>
        [Parameter]
        public ViewT DataSource { get; set; } = new ViewT()
        {
            Entity = new T()
        };
        #endregion

        #region 属性
        /// <summary>
        /// HttpClient
        /// </summary>
        [Inject]
        private HttpService HttpService { get; set; }
        /// <summary>
        /// 全局提示
        /// </summary>
        [Inject]
        protected MessageService MessageService { get; set; }
        [Inject]
        public UserConfig UserConfig { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        public ILanguageService LanguageService => UserConfig.LanguageService;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(CurrentUrl))
            {
                var uri = new Uri(NavigationManager.Uri);
                CurrentUrl = uri.LocalPath[1..];
            }
            await base.OnInitializedAsync();
        }

        #region 方法
        public Form<ViewT> _meunForm;
        /// <summary>
        /// 提交前本地校验
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> Validate()
        {
            if (_meunForm == null)//当_meunForm为null时，不进行表单验证
            {
                return await FormSubmit();
            }
            //数据效验
            if (_meunForm.Validate())
            {
                return await FormSubmit();
            }
            return false;
        }
        /// <summary>
        /// 开始表单提交
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> FormSubmit()
        {
            var result = await HttpService.PostJson(CurrentUrl, DataSource);
            if (result.Status == HttpStatusCode.OK)
            {
                _ = MessageService.Success(result.Title);
                return true;
            }
            return false;
        }


        public RadioOption<enumT>[] GetRadioOptions<enumT>()
        {
            var enumDic = CommonHelper.GetEnumModelHeader<enumT>(typeof(enumT), LanguageService);
            List<RadioOption<enumT>> radioOptions = new List<RadioOption<enumT>>();
            foreach (var item in enumDic)
            {
                radioOptions.Add(new RadioOption<enumT>() { Value = item.Key, Label = item.Value });
            }
            return radioOptions.ToArray();
        }
        #endregion
    }
}