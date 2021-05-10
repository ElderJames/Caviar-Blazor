﻿using Caviar.Models.SystemData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Caviar.Control
{
    public static class CaviarConfig
    {
        public static SqlConfig SqlConfig { get; private set; }

        public static IConfiguration Configuration { get; private set; }

        public static Guid NoLoginRoleGuid { get; private set; }
        public static Guid SysAdminRoleGuid { get; private set; }
        public static int TokenDuration { get; private set; }
        public static string WebUIPath { get; private set; }
        public static string WebUIName { get; private set; }
        public static string ModelsPath { get; private set; }
        public static string SessionUserInfoName { get; private set; } = "SysUserInfo";

        public static bool IsDebug { get; set; }

        static Guid TokenKey;

        public static IServiceCollection AddCaviar(this IServiceCollection services, SqlConfig sqlConfig, IConfiguration configuration)
        {
            SqlConfig = sqlConfig;
            services.AddDbContext<DataContext>();
            services.AddScoped<IDataContext, SysDataContext>();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            Configuration = configuration;

            var caviarDynamicConfig = new CaviarDynamicConfig();
            caviarDynamicConfig.AddIBaseModel(services);
            caviarDynamicConfig.AddInject(services);
            LoadAppsettings();
            return services;
        }

        static void LoadAppsettings()
        {
            var appsettingPath = "appsettings.json";
            string appsettings = "{}";
            if (File.Exists(appsettingPath))
            {
                appsettings = File.ReadAllText(appsettingPath);
            }
            var json = JObject.Parse(appsettings);
            PaseAppsettingsJson(ref json);

            IsDebug = bool.Parse(json["Caviar"]["IsDebug"].ToString());
            var guid = json["Caviar"]["Role"]["NoLoginRole"].ToString();
            NoLoginRoleGuid = Guid.Parse(guid);
            SysAdminRoleGuid = Guid.Parse(json["Caviar"]["Role"]["SysAdminRole"].ToString());
            TokenKey = Guid.Parse(json["Caviar"]["Token"]["Key"].ToString());
            TokenDuration = int.Parse(json["Caviar"]["Token"]["Duration"].ToString());
            WebUIPath = json["Caviar"]["WebUI"]["Path"].ToString();
            WebUIName = json["Caviar"]["WebUI"]["Name"].ToString();
            ModelsPath = json["Caviar"]["Models"]["Path"].ToString();
            var paseJson = json.ToString();
            File.WriteAllText(appsettingPath, paseJson);
        }

        static void PaseAppsettingsJson(ref JObject json)
        {
            if (json["Caviar"] == null) json["Caviar"] = new JObject();
            if (json["Caviar"]["IsDebug"] == null) json["Caviar"]["IsDebug"] = false;
            if (json["Caviar"]["Role"] == null) json["Caviar"]["Role"] = new JObject();
            if (json["Caviar"]["Role"]["NoLoginRole"] == null) json["Caviar"]["Role"]["NoLoginRole"] = Guid.NewGuid();
            if (json["Caviar"]["Role"]["SysAdminRole"] == null) json["Caviar"]["Role"]["SysAdminRole"] = Guid.NewGuid();
            if (json["Caviar"]["Token"] == null) json["Caviar"]["Token"] = new JObject();
            if (json["Caviar"]["Token"]["Duration"] == null) json["Caviar"]["Token"]["Duration"] = 60 * 2;
            if (json["Caviar"]["Token"]["Key"] == null) json["Caviar"]["Token"]["Key"] = Guid.NewGuid();
            if(json["Caviar"]["WebUI"] == null) json["Caviar"]["WebUI"] = new JObject();
            if (json["Caviar"]["WebUI"]["Path"] == null) json["Caviar"]["WebUI"]["Path"] = "../Caviar.Demo.AntDesignUI/";
            if (json["Caviar"]["WebUI"]["Name"] == null) json["Caviar"]["WebUI"]["Name"] = "Caviar.Demo.AntDesignUI";
            if (json["Caviar"]["Models"] == null) json["Caviar"]["Models"] = new JObject();
            if (json["Caviar"]["Models"]["Path"] == null) json["Caviar"]["Models"]["Path"] = "../Caviar.Demo.Models/";
        }


        public static IServiceProvider ApplicationServices { get; set; }

        public static IApplicationBuilder UserCaviar(this IApplicationBuilder app)
        {
            ApplicationServices = app.ApplicationServices;
            return app;
        }

        public static string GetUserToken(UserToken userToken)
        {
            return CommonHelper.SHA256EncryptString(userToken.Id + userToken.UserName + userToken.Uid.ToString() + userToken.CreateTime + TokenKey.ToString());
        }

        /// <summary>
        /// 获取用户的ip地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetUserIp(this HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }
            return ip;
        }
        /// <summary>
        /// 获取请求的完整地址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetAbsoluteUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

        #region IBaseModelExtend扩展




        #endregion


        class CaviarDynamicConfig
        {
            
            /// <summary>
            /// 遍历所有的类，筛选实现IService接口的类，并过判断是否是类,并按照注解方式自动注入类
            /// 自动注入所有继承IDIinjectAtteribute接口的类
            /// </summary>
            public void AddIBaseModel(IServiceCollection services)
            {
                CommonHelper.GetAssembly()
                    //遍历查找
                    .ForEach((t =>
                    {
                        //获取所有对象
                        t.GetTypes()
                            //查找是否包含IDIinjectAtteribute接口的类
                            .Where(u => u.GetInterfaces().Contains(typeof(IDIinjectAtteribute)))
                            //判断是否是类
                            .Where(u => u.IsClass)
                            //转换成list
                            .ToList()
                            //循环,并添注入
                            .ForEach(t =>
                            {
                                services.AddTransient(t);
                            });
                    }));

            }
            /// <summary>
            /// 自动注入所有带有Inject特性类
            /// </summary>
            /// <param name="services"></param>
            public void AddInject(IServiceCollection services)
            {
                CommonHelper.GetAssembly()
                   //遍历查找
                   .ForEach((t =>
                   {
                       //获取所有对象
                       t.GetTypes()
                           //查找是否包含DI特性并且查看是否是抽象类
                           .Where(a => a.GetCustomAttributes(true).Select(t => t.GetType()).Contains(typeof(DIInjectAttribute)) && !a.IsAbstract)
                           //判断是否是类
                           .Where(u => u.IsClass)
                           //转换成list
                           .ToList()
                           //循环,并添注入
                           .ForEach(t =>
                           {
                               var inject = (DIInjectAttribute)t.GetCustomAttributes(true).FirstOrDefault(d => d.GetType() == typeof(DIInjectAttribute));
                               switch (inject.InjectType)
                               {
                                   case InjectType.SINGLETON:
                                       services.AddSingleton(t);
                                       break;
                                   case InjectType.SCOPED:
                                       services.AddScoped(t);
                                       break;
                                   case InjectType.TRANSIENT:
                                       services.AddTransient(t);
                                       break;
                               }
                           });
                   }));
            }
        }
    }



    public class SqlConfig
    {
        public string Connections { get; set; }

        public DBTypeEnum DBTypeEnum { get; set; }
    }
}
