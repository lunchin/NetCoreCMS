﻿/*************************************************************
 *          Project: NetCoreCMS                              *
 *              Web: http://dotnetcorecms.org                *
 *           Author: OnnoRokom Software Ltd.                 *
 *          Website: www.onnorokomsoftware.com               *
 *            Email: info@onnorokomsoftware.com              *
 *        Copyright: OnnoRokom Software Ltd.                 *
 *          License: BSD-3-Clause                            *
 *************************************************************/

using Microsoft.AspNetCore.Mvc.Rendering;
using NetCoreCMS.Framework.Core.Auth;
using NetCoreCMS.Framework.Core.Models;
using NetCoreCMS.Framework.Core.Models.ViewModels;
using NetCoreCMS.Framework.Utility;
using NetCoreCMS.Modules.Admin.Models.ViewModels.UserAuthViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace NetCoreCMS.Modules.Admin.Models.ViewModels
{
    public class UserViewModel
    {
        public long Id { get; set; }

        public long PermissionId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required(ErrorMessage = "The Email field is required")]
        [EmailAddress]
        public string Email { get; set; }

        public string Mobile { get; set; }

        public bool SendEmailNotification { get; set; }

        [Required]
        public string Role { get; set; }

        public static SelectList GetRolesDropdown()
        {
            var list = new Dictionary<string, string>();
            list.Add(NccCmsRoles.Administrator, NccCmsRoles.Administrator);
            list.Add(NccCmsRoles.Author, NccCmsRoles.Author);
            list.Add(NccCmsRoles.Contributor, NccCmsRoles.Contributor);
            list.Add(NccCmsRoles.Editor, NccCmsRoles.Editor);
            list.Add(NccCmsRoles.Subscriber, NccCmsRoles.Subscriber);
            list.Add(NccCmsRoles.SuperAdmin, NccCmsRoles.SuperAdmin);

            return  new SelectList(list, "Value","Key");
        }

        public List<ModuleViewModel> DenyModules { get; set; }
        public List<ModuleViewModel> AllowModules { get; set; }
        private NccPermission _permission;

        public UserViewModel()
        {
            DenyModules = new List<ModuleViewModel>();
            AllowModules = new List<ModuleViewModel>();

            foreach (var item in GlobalContext.GetActiveModules())
            {
                var module = new ModuleViewModel();

                var menus = item.Menus;
                var adminMenus = menus
                    .Where(x => x.Type == Menu.MenuType.Admin)
                    .GroupBy(y => y.DisplayName,
                        (key, g) => new { MenuName = key, Menu = g.FirstOrDefault(), Items = g.SelectMany(x => x.MenuItems).ToList() }
                    ).ToList();

                var siteMenus = menus.Where(x => x.Type == Menu.MenuType.WebSite)
                    .GroupBy(y => y.DisplayName,
                        (key, g) => new { MenuName = key, Menu = g.FirstOrDefault(), Items = g.SelectMany(z => z.MenuItems).ToList() }
                    ).ToList();

                foreach (var adminMenu in adminMenus)
                {
                    var menu = new MenuViewModel()
                    {
                        Type = "Admin",
                        Name = adminMenu.MenuName,
                        Order = adminMenu.Menu.Order,
                        MenuItems = GetMenuItems(adminMenu.Items, item.ModuleId),
                    };
                    module.AdminMenus.Add(menu);
                }

                foreach (var webSiteMenu in siteMenus)
                {
                    var menu = new MenuViewModel()
                    {
                        Type = "WebSite",
                        Name = webSiteMenu.MenuName,
                        Order = webSiteMenu.Menu.Order,
                        MenuItems = GetMenuItems(webSiteMenu.Items, item.ModuleId),
                    };
                    module.SiteMenus.Add(menu);
                }
                DenyModules.Add(module);
                AllowModules.Add(module);
            }
        }

        public UserViewModel(NccPermission permission)
        {
            _permission = permission;
            PermissionId = permission.Id;
            
            DenyModules = new List<ModuleViewModel>();
            AllowModules = new List<ModuleViewModel>();

            foreach (var item in GlobalContext.GetActiveModules())
            {
                var module = new ModuleViewModel();

                var menus = item.Menus;
                var adminMenus = menus
                    .Where(x => x.Type == Menu.MenuType.Admin)
                    .GroupBy(y => y.DisplayName,
                        (key, g) => new { MenuName = key, Menu = g.FirstOrDefault(), Items = g.SelectMany(x => x.MenuItems).ToList() }
                    ).ToList();

                var siteMenus = menus.Where(x => x.Type == Menu.MenuType.WebSite)
                    .GroupBy(y => y.DisplayName,
                        (key, g) => new { MenuName = key, Menu = g.FirstOrDefault(), Items = g.SelectMany(z => z.MenuItems).ToList() }
                    ).ToList();

                foreach (var adminMenu in adminMenus)
                {
                    var menu = new MenuViewModel()
                    {
                        Type = "Admin",
                        Name = adminMenu.MenuName,
                        Order = adminMenu.Menu.Order,
                        MenuItems = GetMenuItems(adminMenu.Items, item.ModuleId),
                    };
                    module.AdminMenus.Add(menu);
                }

                foreach (var webSiteMenu in siteMenus)
                {
                    var menu = new MenuViewModel()
                    {
                        Type = "WebSite",
                        Name = webSiteMenu.MenuName,
                        Order = webSiteMenu.Menu.Order,
                        MenuItems = GetMenuItems(webSiteMenu.Items, item.ModuleId),
                    };
                    module.SiteMenus.Add(menu);
                }
                DenyModules.Add(module);
                AllowModules.Add(module);
            }
        }

        private List<MenuItemViewModel> GetMenuItems(List<MenuItem> menuItems, string moduleId)
        {
            var list = new List<MenuItemViewModel>();
            foreach (var item in menuItems)
            {
                string controller = "", action = "";
                if (string.IsNullOrEmpty(item.Url) == false)
                {
                    var parts = item.Url.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        controller = parts[0];
                        action = parts[1];
                    }
                    else if (parts.Length == 1)
                    {
                        controller = parts[0];
                        action = "Index";
                    }
                }

                var mip = _permission?.PermissionDetails?.Where(x => x.ModuleId == moduleId && x.Action == action && x.Controller == controller).FirstOrDefault();
                if (mip != null)
                {
                    var mi = new MenuItemViewModel()
                    {
                        Id = mip.Id,
                        Action = action,
                        Controller = controller,
                        Name = mip.Name,
                        Order = mip.Order,
                        IsChecked = true,
                    };
                    list.Add(mi);
                }
                else
                {
                    var mi = new MenuItemViewModel()
                    {
                        Id = 0,
                        Action = action,
                        Controller = controller,
                        Name = item.Name,
                        Order = item.Order
                    };
                    list.Add(mi);
                }
            }
            return list;
        }
    }
}
