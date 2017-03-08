using System;
using Microsoft.ServiceFabric.Actors;

namespace ServiceFabric.Mocks
{
          

    public class MissingActorEventArgs : EventArgs
    {
        public Type ActorType { get; set; }

        public ActorId Id { get; set; }
    }
    
}