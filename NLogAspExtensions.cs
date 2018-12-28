/* ------------------------------------------------------------------------- *
thZero.NetCore.Library.Services.Logging.NLog
Copyright (C) 2016-2018 thZero.com

<development [at] thzero [dot] com>

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 * ------------------------------------------------------------------------- */

using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Microsoft.Extensions.Logging
{
    public static class NLogAspExtensions
    {
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, WebHostBuilderContext hostingContext, Action<NLogAspNetCoreOptions> optionsFunc = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostingContext == null)
                throw new ArgumentNullException(nameof(hostingContext));

            var aspnetEnvironment = hostingContext.HostingEnvironment.EnvironmentName;
            aspnetEnvironment = !string.IsNullOrEmpty(aspnetEnvironment?.Trim()) ? aspnetEnvironment.Trim() : "Production";

            var environmentSpecificLogFileName = System.IO.Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, $"nlog.{aspnetEnvironment}.config");
            hostingContext.HostingEnvironment.ConfigureNLog(environmentSpecificLogFileName);

            var options = NLogAspNetCoreOptions.Default;
            optionsFunc?.Invoke(options);

            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(AspNetExtensions).GetType().Assembly);
            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).GetType().Assembly);

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider>(new NLogLoggerProvider(options)));

            if (options.RegisterHttpContextAccessor)
                builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpContextAccessor, HttpContextAccessor>());

            return builder;
        }
    }
}