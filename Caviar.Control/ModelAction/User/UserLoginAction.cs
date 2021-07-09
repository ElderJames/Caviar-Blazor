﻿using Caviar.Control.ModelAction;
using Caviar.Models.SystemData;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caviar.Control.User
{
    public partial class UserAction
    {
        /// <summary>
        /// 登录成功返回token，失败返回错误原因
        /// </summary>
        /// <returns></returns>
        public virtual ResultMsg<UserToken> Login(ViewUser entity)
        {

            if (string.IsNullOrEmpty(entity.Password) || entity.Password.Length != 64) return Error<UserToken>("用户名或密码错误");
            SysUser userLogin = null;
            if (!string.IsNullOrEmpty(entity.UserName))
            {
                userLogin = BC.DC.GetSingleEntityAsync<SysUser>(u => u.UserName == entity.UserName && u.Password == entity.Password).Result;
            }
            else if (!string.IsNullOrEmpty(entity.PhoneNumber))
            {
                userLogin = BC.DC.GetSingleEntityAsync<SysUser>(u => u.PhoneNumber == entity.PhoneNumber && u.Password == entity.Password).Result;
            }
            if (userLogin == null) return Error<UserToken>("用户名或密码错误");
            BC.UserToken.AutoAssign(userLogin);
            BC.UserToken.CreateTime = DateTime.Now;
            BC.UserToken.Token = CaviarConfig.GetUserToken(BC.UserToken);
            BC.UserToken.Duration = CaviarConfig.TokenConfig.Duration;
            return Ok("登录成功，欢迎回来",BC.UserToken);
        }

        public virtual ResultMsg Register(ViewUser entity)
        {
            var user = BC.DC.GetSingleEntityAsync<SysUser>(u => u.UserName == entity.UserName).Result;
            if (user != null)
            {
                return Error("该用户名已经被注册!");
            }
            user = BC.DC.GetSingleEntityAsync<SysUser>(u => u.PhoneNumber == entity.PhoneNumber).Result;
            if (user != null)
            {
                return Error("该手机号已经被注册!");
            }
            var count = BC.DC.AddEntityAsync(entity).Result;
            if (count <= 0)
            {
                return Error("注册账号失败!");
            }
            return Ok("账号注册成功");
        }
    }
}
