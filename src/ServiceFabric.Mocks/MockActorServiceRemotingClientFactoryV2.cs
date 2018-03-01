using System;
using System.Collections.Generic;
using System.Fabric;
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
        /// The <see cref="IServiceRemotingMessageBodyFactory"/> v2 to return from <see cref="GetRemotingMessageBodyFactory"/>.
        /// </summary>
        public IServiceRemotingMessageBodyFactory MockServiceRemotingMessageBodyFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IServiceRemotingClient"/> to return from GetClientAsync
        /// </summary>
        public IServiceRemotingClient ServiceRemotingClient { get; set; }

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
            var remotingClient = ServiceRemotingClient ?? new MockActorServiceRemotingClient(_wrappedService)
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
        /// <summary>
        /// Gets or sets the request to return from <see cref="CreateRequest"/>.
        /// </summary>
        public IServiceRemotingRequestMessageBody Request { get; set; }

        /// <summary>
        /// Gets or sets the response to return from <see cref="CreateResponse"/>.
        /// </summary>
        public IServiceRemotingResponseMessageBody Response { get; set; }

        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters)
        {
            return Request;
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName)
        {
            return Response;
        }
    }

    public class MockServiceRemotingRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        //public int Positition { get; set; }

        //public string ParameName { get; set; }

        //public object Parameter { get; set; }

        public Dictionary<int, Dictionary<string, object>> StoredValues { get; } = new Dictionary<int, Dictionary<string, object>>();

        public void SetParameter(int position, string parameName, object parameter)
        {
            if (!StoredValues.TryGetValue(position, out var dict))
            {
                dict = new Dictionary<string, object>();
                StoredValues.Add(position, dict);
            }
            if (!dict.TryGetValue(parameName, out var val))
            {
                dict.Add(parameName, parameter);
            }
            else
            {
                dict[parameName] = val;
            }
        }

        public object GetParameter(int position, string parameName, Type paramType)
        {
            if (StoredValues.TryGetValue(position, out var dict)
                && dict.TryGetValue(parameName, out var val))
            {
                return val;
            }
            return null;
        }
    }

    public class MockServiceRemotingResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        public object Response { get; set; }

        public void Set(object response)
        {
            Response = response;
        }

        public object Get(Type paramType)
        {
            return Response;
        }
    }


    /// <summary>
    /// Mock implementation of <see cref="IServiceRemotingClient"/> v2. (returned from <see cref="MockActorServiceRemotingClientFactory"/>)
    /// Defines the interface that must be implemented to provide a client for Service Remoting communication.
    /// </summary>
    public class MockActorServiceRemotingClient : IServiceRemotingClient
    {
        /// <summary>
        /// Null
        /// </summary>
        public ResolvedServiceEndpoint Endpoint { get; set; }

        /// <inheritdoc />
        public string ListenerName { get; set; }

        /// <summary>
        /// Null
        /// </summary>
        public ResolvedServicePartition ResolvedServicePartition { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ServicePartitionKey"/>.
        /// </summary>
        public ServicePartitionKey PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TargetReplicaSelector"/>.
        /// </summary>
        public TargetReplicaSelector TargetReplicaSelector { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OperationRetrySettings"/>.
        /// </summary>
        public OperationRetrySettings RetrySettings { get; set; }

        /// <summary>
        /// Gets or sets the wrapped <see cref="ActorService"/>.
        /// </summary>
        public ActorService WrappedService { get; }

        /// <summary>
        /// Gets or sets the <see cref="IServiceRemotingResponseMessage"/> to return from <see cref="RequestResponseAsync"/>
        /// Hint; use <see cref="MockServiceRemotingResponseMessage"/>
        /// </summary>
        public IServiceRemotingResponseMessage ServiceRemotingResponseMessage { get; set; }


        public MockActorServiceRemotingClient(ActorService wrappedService)
        {
            if (wrappedService == null) throw new ArgumentNullException(nameof(wrappedService));
            WrappedService = wrappedService;
        }


        /// <inheritdoc />
        public Task<IServiceRemotingResponseMessage> RequestResponseAsync(IServiceRemotingRequestMessage requestRequestMessage)
        {
            return Task.FromResult(ServiceRemotingResponseMessage);

        }

        /// <inheritdoc />
        public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Mock implementation of <see cref="IServiceRemotingResponseMessage"/>
    /// </summary>
    public class MockServiceRemotingResponseMessage : IServiceRemotingResponseMessage
    {
        public IServiceRemotingResponseMessageHeader Header { get; set; }

        public IServiceRemotingResponseMessageBody Body { get; set; }

        public IServiceRemotingResponseMessageHeader GetHeader()
        {
            return Header;
        }

        public IServiceRemotingResponseMessageBody GetBody()
        {
            return Body;
        }
    }
}