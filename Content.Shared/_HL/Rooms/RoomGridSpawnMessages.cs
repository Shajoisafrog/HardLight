using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._HL.Rooms;

[Serializable, NetSerializable]
public sealed class RequestRoomGridLoadMessage : EntityEventArgs
{
    public NetEntity ConsoleNetEntity { get; }
    public string CharacterKey { get; }

    public RequestRoomGridLoadMessage(NetEntity consoleNetEntity, string characterKey)
    {
        ConsoleNetEntity = consoleNetEntity;
        CharacterKey = characterKey;
    }
}

[Serializable, NetSerializable]
public sealed class SendRoomGridDataMessage : EntityEventArgs
{
    public NetEntity ConsoleNetEntity { get; }
    public string CharacterKey { get; }
    public string RoomData { get; }
    public bool Found { get; }

    public SendRoomGridDataMessage(NetEntity consoleNetEntity, string characterKey, string roomData, bool found)
    {
        ConsoleNetEntity = consoleNetEntity;
        CharacterKey = characterKey;
        RoomData = roomData;
        Found = found;
    }
}

[Serializable, NetSerializable]
public sealed class SendRoomGridSaveDataClientMessage : EntityEventArgs
{
    public string CharacterKey { get; }
    public string RoomData { get; }

    public SendRoomGridSaveDataClientMessage(string characterKey, string roomData)
    {
        CharacterKey = characterKey;
        RoomData = roomData;
    }
}
