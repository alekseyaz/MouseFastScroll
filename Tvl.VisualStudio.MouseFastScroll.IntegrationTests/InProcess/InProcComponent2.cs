﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Tvl.VisualStudio.MouseFastScroll.IntegrationTests.InProcess
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.VisualStudio.Threading;
    using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;
    using DTE = EnvDTE.DTE;
    using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.Interop.IAsyncServiceProvider;
    using SAsyncServiceProvider = Microsoft.VisualStudio.Shell.Interop.SAsyncServiceProvider;
    using SDTE = Microsoft.VisualStudio.Shell.Interop.SDTE;
    using ServiceProvider = Microsoft.VisualStudio.Shell.ServiceProvider;

    internal abstract class InProcComponent2
    {
        protected InProcComponent2(JoinableTaskFactory joinableTaskFactory)
        {
            JoinableTaskFactory = joinableTaskFactory ?? throw new ArgumentNullException(nameof(joinableTaskFactory));
        }

        protected JoinableTaskFactory JoinableTaskFactory
        {
            get;
        }

        protected async Task<TInterface> GetGlobalServiceAsync<TService, TInterface>()
            where TService : class
            where TInterface : class
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            if (ServiceProvider.GlobalProvider.GetService(typeof(SAsyncServiceProvider)) is IAsyncServiceProvider asyncServiceProvider)
            {
                return (TInterface)await asyncServiceProvider.QueryServiceAsync(typeof(TService).GUID);
            }
            else
            {
                return (TInterface)ServiceProvider.GlobalProvider.GetService(typeof(TService));
            }
        }

        protected async Task<DTE> GetDTEAsync()
        {
            return await GetGlobalServiceAsync<SDTE, DTE>();
        }

        /// <summary>
        /// Waiting for the application to 'idle' means that it is done pumping messages (including WM_PAINT).
        /// </summary>
        protected internal static async Task WaitForApplicationIdleAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }
    }
}
