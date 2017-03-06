using System;
using Microsoft.ServiceFabric.Actors;

namespace ServiceFabric.Mocks
{
          

    public class MisingActorEventArgs : EventArgs
    {
        public ActorId Id { get; set; }
    }
    
}