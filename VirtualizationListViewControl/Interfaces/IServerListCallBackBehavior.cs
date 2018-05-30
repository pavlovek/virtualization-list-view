using System;
using System.Collections.Generic;
using VirtualizationListViewControl.ServerListChangesCallBack;

namespace VirtualizationListViewControl.Interfaces
{
    /// <summary>
    /// ItemsProvider call back behavior interface
    /// </summary>
    /// <typeparam name="T">Data item object</typeparam>
    public interface IServerListCallBackBehavior<T> : IDisposable
    {
        /// <summary>
        /// List changed event
        /// </summary>
        event Action<List<ServerListChanging<T>>> ListUpdates;
    }
}
