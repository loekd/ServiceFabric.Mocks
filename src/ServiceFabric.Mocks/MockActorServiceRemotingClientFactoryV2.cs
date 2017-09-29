using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;

// ReSharper disable once CheckNamespace
namespace ServiceFabric.Mocks.RemotingV2
{
    public class MockActorServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        private readonly ActorService _wrappedService;
        private readonly Dictionary<IServiceRemotingClient, ExceptionInformation> _reportedExceptionInformation = new Dictionary<IServiceRemotingClient, ExceptionInformation>();
        private readonly Dictionary<IServiceRemotingClient, OperationRetryControl> _operationRetryControls = new Dictionary<IServiceRemotingClient, OperationRetryControl>();

        /// <summary>
        /// The <see cref="IServiceRemotingMessageBodyFactory"/> to return from <see cref="GetRemotingMessageBodyFactory"/>.
        /// </summary>
        public IServiceRemotingMessageBodyFactory MockServiceRemotingMessageBodyFactory { get; set; }


        /// <summary>
        /// Manually invoke using <see cref="OnClientConnected"/>
        /// </summary>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

        /// Manually invoke using <see cref="OnClientDisconnected"/>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        /// <summary>
        /// Contains the last reported exception information.
        /// </summary>
        public IReadOnlyDictionary<IServiceRemotingClient, ExceptionInformation> ReportedExceptionInformation => _reportedExceptionInformation;

        public MockActorServiceRemotingClientFactory(ActorService wrappedService)
        {
            _wrappedService = wrappedService;
        }

        public Task<IServiceRemotingClient> GetClientAsync(Uri serviceUri, ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector,
            string listenerName, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            var remotingClient = new MockActorServiceRemotingClient(_wrappedService)
            {
                ListenerName = listenerName,
                PartitionKey = partitionKey,
                TargetReplicaSelector = targetReplicaSelector,
                RetrySettings = retrySettings
            };

            return Task.FromResult<IServiceRemotingClient>(remotingClient);
        }

        public Task<IServiceRemotingClient> GetClientAsync(ResolvedServicePartition previousRsp, TargetReplicaSelector targetReplicaSelector,
            string listenerName, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            ServicePartitionKey partitionKey;
            switch (previousRsp.Info.Kind)
            {
                case ServicePartitionKind.Singleton:
                    partitionKey = new ServicePartitionKey();
                    break;
                case ServicePartitionKind.Int64Range:
                    partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)previousRsp.Info).LowKey);
                    break;
                case ServicePartitionKind.Named:
                    partitionKey = new ServicePartitionKey(((NamedPartitionInformation)previousRsp.Info).Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return GetClientAsync(previousRsp.ServiceName, partitionKey,
                targetReplicaSelector, listenerName, retrySettings, cancellationToken);

        }

        public Task<OperationRetryControl> ReportOperationExceptionAsync(IServiceRemotingClient client, ExceptionInformation exceptionInformation,
            OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            _reportedExceptionInformation[client] = exceptionInformation;
            OperationRetryControl operationRetryControl;
            _operationRetryControls.TryGetValue(client, out operationRetryControl);
            return Task.FromResult(operationRetryControl);
        }

        /// <summary>
        /// Triggers the event <see cref="ClientConnected"/>
        /// </summary>
        /// <param name="e"></param>
        public void OnClientConnected(CommunicationClientEventArgs<IServiceRemotingClient> e)
        {
            ClientConnected?.Invoke(this, e);
        }

        /// <summary>
        /// Triggers the event <see cref="OnClientDisconnected"/>
        /// </summary>
        /// <param name="e"></param>
        public void OnClientDisconnected(CommunicationClientEventArgs<IServiceRemotingClient> e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Registers an <see cref="OperationRetryControl"/> to return from <see cref="ReportOperationExceptionAsync"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="control"></param>
        public void RegisterOperationRetryControl(IServiceRemotingClient client, OperationRetryControl control)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _operationRetryControls[client] = control;
        }
        public IServiceRemotingMessageBodyFactory GetRemotingMessageBodyFactory()
        {
            return MockServiceRemotingMessageBodyFactory;
        }

    }

    public class MockServiceRemotingMessageBodyFactory : IServiceRemotingMessageBodyFactory
    {
        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters)
        {
            return new MockServiceRemotingRequestMessageBody(interfaceName, methodName, numberOfParameters);
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName)
        {
            return new MockServiceRemotingResponseMessageBody(interfaceName, methodName);
        }
    }

    public class MockServiceRemotingRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        public string InterfaceName { get; }
        public string MethodName { get; }
        public int NumberOfParameters { get; }

        private readonly List<Tuple<int, string, object>> _parameters = new List<Tuple<int, string, object>>();

        public MockServiceRemotingRequestMessageBody(string interfaceName, string methodName, int numberOfParameters)
        {
            InterfaceName = interfaceName;
            MethodName = methodName;
            NumberOfParameters = numberOfParameters;
        }
        public void SetParameter(int position, string parameName, object parameter)
        {
            _parameters.Add(new Tuple<int, string, object>(position, parameName, parameter));
        }

        public object GetParameter(int position, string parameName, Type paramType)
        {
            return _parameters.FirstOrDefault(p => p.Item1 == position && p.Item2 == parameName)?.Item3;
        }
    }

    public class MockServiceRemotingResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        public string InterfaceName { get; }
        public string MethodName { get; }

        private readonly Dictionary<Type, object> _values = new Dictionary<Type, object>();

        public MockServiceRemotingResponseMessageBody(string interfaceName, string methodName)
        {
            InterfaceName = interfaceName;
            MethodName = methodName;
        }

        public void Set(object response)
        {
            _values.Add(response.GetType(), response);
        }

        public object Get(Type paramType)
        {
            if (_values.TryGetValue(paramType, out var result))
            {
                return result;
            }
            return null;
        }
    }
}