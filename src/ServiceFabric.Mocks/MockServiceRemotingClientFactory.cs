using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ServiceFabric.Mocks
{
	/// <summary>
	/// Mock implementation of <see cref="IServiceRemotingClientFactory"/>.
	/// Defines the interface that must be implemented for providing the remoting communication client factory.
	/// </summary>
	public class MockActorServiceRemotingClientFactory : IServiceRemotingClientFactory
	{
		private readonly ActorService _wrappedService;
		private readonly Dictionary<IServiceRemotingClient, ExceptionInformation> _reportedExceptionInformation = new Dictionary<IServiceRemotingClient, ExceptionInformation>();
		private readonly Dictionary<IServiceRemotingClient, OperationRetryControl> _operationRetryControls = new Dictionary<IServiceRemotingClient, OperationRetryControl>();

		/// <summary>
		/// Contains the last reported exception information.
		/// </summary>
		public IReadOnlyDictionary<IServiceRemotingClient, ExceptionInformation> ReportedExceptionInformation => _reportedExceptionInformation;

		public MockActorServiceRemotingClientFactory(ActorService wrappedService)
		{
			if (wrappedService == null) throw new ArgumentNullException(nameof(wrappedService));
			_wrappedService = wrappedService;
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
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

		/// <summary>
		/// Registers the provided <paramref name="exceptionInformation"/> in <see cref="ReportedExceptionInformation"/> and return the registered <see cref="OperationRetryControl"/>. (using <see cref="RegisterOperationRetryControl"/>)
		/// </summary>
		/// <param name="client"></param>
		/// <param name="exceptionInformation"></param>
		/// <param name="retrySettings"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<OperationRetryControl> ReportOperationExceptionAsync(IServiceRemotingClient client, ExceptionInformation exceptionInformation,
			OperationRetrySettings retrySettings, CancellationToken cancellationToken)
		{
			_reportedExceptionInformation[client] = exceptionInformation;
			OperationRetryControl operationRetryControl;
			_operationRetryControls.TryGetValue(client, out operationRetryControl);
			return Task.FromResult(operationRetryControl);
		}

		/// <summary>
		/// Manually invoke using <see cref="OnClientConnected"/>
		/// </summary>
		public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

		/// Manually invoke using <see cref="OnClientDisconnected"/>
		public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

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
	}

	/// <summary>
	/// Mock implementation of <see cref="IServiceRemotingClient"/>. (returned from <see cref="MockActorServiceRemotingClientFactory"/>)
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
		/// Gets or sets the wrapped <see cref="IService"/>.
		/// </summary>
		public ActorService WrappedService { get; }

		
		public MockActorServiceRemotingClient(ActorService wrappedService)
		{
			if (wrappedService == null) throw new ArgumentNullException(nameof(wrappedService));
			WrappedService = wrappedService;
		}

		/// <inheritdoc />
		public Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
		{
			return Task.FromResult(new byte[0]);
		}

		/// <inheritdoc />
		public void SendOneWay(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
		{
		}
	}
}
