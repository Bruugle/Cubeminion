using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;

namespace Networking
{
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        public NetworkVariable<bool> DisablePositionSync = new NetworkVariable<bool>(false);

        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            DisablePositionSync.OnValueChanged += OnDisablePositionSync;
            OnDisablePositionSync(false, DisablePositionSync.Value);

        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            DisablePositionSync.OnValueChanged -= OnDisablePositionSync;
        }

        void OnDisablePositionSync(bool previous, bool next)
        {
            if (next)
            {
                SyncPositionX = false; SyncPositionY = false; SyncPositionZ = false;
            }
            else
            {
                SyncPositionX = true; SyncPositionY = true; SyncPositionZ = true;
            }
        }

    }
}


