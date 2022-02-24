﻿using AntDesign;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caviar.SharedKernel.Entities.View;
using System.Net;
using Caviar.SharedKernel.Entities;

namespace Caviar.AntDesignUI.Pages.Menu
{
    public partial class DataTemplate
    {
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _sysMenus = await GetMenus();
            _listMenus = TreeToList(_sysMenus);
            CheckMenuType();
        }

        private List<SysMenuView> _sysMenus = new List<SysMenuView>();

        private List<SysMenuView> _listMenus;
        

        async Task<List<SysMenuView>> GetMenus()
        {

            var result = await HttpService.GetJson<PageData<SysMenuView>>($"{Url[CurrencyConstant.SysMenuKey]}?pageSize=100");
            if (result.Status != HttpStatusCode.OK) return null;
            
            return result.Data.Rows;
        }

        List<SysMenuView> TreeToList(List<SysMenuView> menus)
        {
            List<SysMenuView> listData = new List<SysMenuView>();
            menus.TreeToList(listData);
            if (DataSource.ParentId > 0)
            {
                var parent = listData.SingleOrDefault(u => u.Id == DataSource.ParentId);
                if (parent != null)
                {
                    ParentMenuName = parent.DisplayName;
                }
            }
            return listData;
        }

        string ParentMenuName { get; set; } = "无上层目录";
        void EventRecord(TreeEventArgs<SysMenuView> args)
        {
            ParentMenuName = args.Node.Title;
            DataSource.Entity.ParentId = int.Parse(args.Node.Key);
            var parent = _listMenus.SingleOrDefault(u => u.Id == DataSource.Entity.ParentId);
            if (parent != null)
            {
                DataSource.Entity.ControllerName = parent.Entity.ControllerName;
            }
        }

        void RemoveRecord()
        {
            ParentMenuName = "无上层目录";
            DataSource.Entity.ParentId = 0;
        }
    }
}
