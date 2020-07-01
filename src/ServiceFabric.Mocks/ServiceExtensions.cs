using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Mocks
{
    /// <summary>
    /// Extension methods for Services.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Invokes OnOpenAsync on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="replicaOpenMode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task InvokeOnOpenAsync(this StatefulServiceBase service,
            ReplicaOpenMode replicaOpenMode = ReplicaOpenMode.Existing,
            CancellationToken? cancellationToken = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected virtual Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
            var method = FindMethodInfo(service, "OnOpenAsync");
            return (Task)method.Invoke(service, new object[] { replicaOpenMode, cancellationToken ?? CancellationToken.None });
        }

        /// <summary>
        /// Invokes OnOpenAsync on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task InvokeOnOpenAsync(this StatelessService service,
            CancellationToken? cancellationToken = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected virtual Task OnOpenAsync(CancellationToken cancellationToken)
            var method = FindMethodInfo(service, "OnOpenAsync");
            return (Task)method.Invoke(service, new object[] { cancellationToken ?? CancellationToken.None });
        }

        /// <summary>
        /// Invokes RunAsync on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task InvokeRunAsync(this StatefulServiceBase service, CancellationToken? cancellationToken = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected virtual Task RunAsync(CancellationToken cancellationToken)
            var method = FindMethodInfo(service, "RunAsync");
            var task = Task.Run(() =>
            {
                Task inner = null;
                try
                {
                    inner = (Task)method.Invoke(service, new object[] { cancellationToken ?? CancellationToken.None });
                }
                catch (TargetInvocationException ex)
                {
                    //If an OperationCanceledException escapes from RunAsync(CancellationToken) and 
                    //Service Fabric runtime has requested cancellation by signaling cancellationToken 
                    //passed to RunAsync(CancellationToken), Service Fabric runtime handles this 
                    //exception and considers it as graceful completion of RunAsync(CancellationToken).
                    if (!(ex.InnerException is OperationCanceledException))
                    {
                        //otherwise, we don't know what happened...
                        throw;
                    }
                    inner = Task.FromResult(true);
                }
                return inner;
            });
            return task;
        }

        /// <summary>
        /// Invokes RunAsync on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task InvokeRunAsync(this StatelessService service, CancellationToken? cancellationToken = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected virtual Task RunAsync(CancellationToken cancellationToken)
            var method = FindMethodInfo(service, "RunAsync");
            var task = Task.Run(() =>
            {
                Task inner = null;
                try
                {
                    inner = (Task)method.Invoke(service, new object[] { cancellationToken ?? CancellationToken.None });
                }
                catch (TargetInvocationException ex)
                {
                    //If an OperationCanceledException escapes from RunAsync(CancellationToken) and 
                    //Service Fabric runtime has requested cancellation by signaling cancellationToken 
                    //passed to RunAsync(CancellationToken), Service Fabric runtime handles this 
                    //exception and considers it as graceful completion of RunAsync(CancellationToken).
                    if (!(ex.InnerException is OperationCanceledException))
                    {
                        //otherwise, we don't know what happened...
                        throw;
                    }
                }
                return inner;
            });
            return task;
        }


        /// <summary>
        /// Invokes OnChangeRoleAsync on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="newRole"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task InvokeOnChangeRoleAsync(this StatefulServiceBase service, ReplicaRole newRole, CancellationToken? cancellationToken = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected virtual Task RunAsync(CancellationToken cancellationToken)
            var method = FindMethodInfo(service, "OnChangeRoleAsync");
            return (Task)method.Invoke(service, new object[] { newRole, cancellationToken ?? CancellationToken.None });
        }

        /// <summary>
        /// Invokes OnCloseAsync on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task InvokeOnCloseAsync(this StatefulServiceBase service, CancellationToken? cancellationToken = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            // protected virtual Task OnCloseAsync(CancellationToken cancellationToken)
            var method = FindMethodInfo(service, "OnCloseAsync");
            return (Task)method.Invoke(service, new object[] { cancellationToken ?? CancellationToken.None });
        }

        /// <summary>
        /// Invokes OnCloseAsync on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task InvokeOnCloseAsync(this StatelessService service, CancellationToken? cancellationToken = null)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            // protected virtual Task OnCloseAsync(CancellationToken cancellationToken)
            var method = FindMethodInfo(service, "OnCloseAsync");
            return (Task)method.Invoke(service, new object[] { cancellationToken ?? CancellationToken.None });
        }

        /// <summary>
        /// Invokes CreateServiceReplicaListeners on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IEnumerable<ServiceReplicaListener> InvokeCreateServiceReplicaListeners(this StatefulServiceBase service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected virtual IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
            var method = FindMethodInfo(service, "CreateServiceReplicaListeners");
            return (IEnumerable<ServiceReplicaListener>)method.Invoke(service, null);
        }

        /// <summary>
        /// Invokes CreateServiceInstanceListeners on the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IEnumerable<ServiceInstanceListener> InvokeCreateServiceInstanceListeners(this StatelessService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected virtual IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
            var method = FindMethodInfo(service, "CreateServiceInstanceListeners");
            return (IEnumerable<ServiceInstanceListener>)method.Invoke(service, null);
        }

        /// <summary>
        /// Gets the partition info of the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IStatefulServicePartition GetPartition(this StatefulServiceBase service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected IStatefulServicePartition Partition { get; private set; }
            var propertyInfo = typeof(StatefulServiceBase).GetProperty("Partition", BindingFlags.Instance | BindingFlags.NonPublic);
            return (IStatefulServicePartition)propertyInfo?.GetValue(service, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sets the partition info of the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="partition">partition to set</param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static void SetPartition(this StatefulServiceBase service, IStatefulServicePartition partition)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected IStatefulServicePartition Partition { get; private set; }
            var propertyInfo = typeof(StatefulServiceBase).GetProperty("Partition", BindingFlags.Instance | BindingFlags.NonPublic);
            propertyInfo?.SetValue(service, partition, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the partition info of the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IStatelessServicePartition GetPartition(this StatelessService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected IStatelessServicePartition Partition { get; private set; }
            var propertyInfo = typeof(StatelessService).GetProperty("Partition", BindingFlags.Instance | BindingFlags.NonPublic);
            return (IStatelessServicePartition)propertyInfo?.GetValue(service, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sets the partition info of the provided <paramref name="service"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="partition">partition to set</param>
        /// <returns></returns>
        public static void SetPartition(this StatelessService service, IStatelessServicePartition partition)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            //protected IStatelessServicePartition Partition { get; private set; }
            var propertyInfo = typeof(StatelessService).GetProperty("Partition", BindingFlags.Instance | BindingFlags.NonPublic);
            propertyInfo?.SetValue(service, partition, BindingFlags.Instance | BindingFlags.NonPublic, null, null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Finds non private instance method on <paramref name="service"/> by name.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static MethodInfo FindMethodInfo(object service, string methodName)
        {
            var method = service.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new Exception($"Unable to find method '{methodName}' on service '{service.GetType().FullName}'");
            return method;
        }
    }
}
