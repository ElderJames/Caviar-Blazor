﻿using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
namespace Caviar.SharedKernel
{
    public interface ILanguageService
    {
        CultureInfo CurrentCulture { get; }

        JObject Resources { get; }

        event EventHandler<CultureInfo> LanguageChanged;

        void SetLanguage(CultureInfo culture);

        string this[string name] { get; }

        /// <summary>
        /// 获取支持的语言列表
        /// </summary>
        /// <returns></returns>
        string[] GetLanguageList();
    }
}
