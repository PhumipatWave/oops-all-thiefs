using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct LeaderboardState : INetworkSerializable, IEquatable<LeaderboardState>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Moneys;

    public bool Equals(LeaderboardState other)
    {
        return ClientId == other.ClientId &&
            PlayerName.Equals(other.PlayerName) && 
            Moneys == other.Moneys;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Moneys);
    }
}
