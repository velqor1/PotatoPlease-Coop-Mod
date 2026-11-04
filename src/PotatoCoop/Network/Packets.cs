using System;
using System.IO;
using UnityEngine;

namespace PotatoCoop.Network
{
    public enum PacketType : byte
    {
        Handshake = 1,
        HandshakeAck = 2,
        PlayerTransform = 3,
        QuestSync = 4,
        VoxelCut = 5,
        MachineSync = 6,
        DoorSync = 7,
        EconomySync = 8,
        PlayerLeave = 9,
        ChatMessage = 10,
        StartShift = 11,
        UpgradeSync = 12,
        SackRipped = 13,
        PotatoClean = 14,
        QuestBump = 15,
        QuestFlag = 16,
        QuestPage = 17
    }

    public static class PacketSerializer
    {
        public static byte[] SerializeStartShift(int difficulty = 0)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.StartShift);
            writer.Write(difficulty);
            return ms.ToArray();
        }

        public static byte[] SerializeHandshake(ulong steamId, string playerName, int skinIndex)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.Handshake);
            writer.Write(steamId);
            writer.Write(playerName ?? "Worker");
            writer.Write(skinIndex);
            return ms.ToArray();
        }

        public static byte[] SerializeHandshakeAck(int assignedIndex, int questIndex, int money)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.HandshakeAck);
            writer.Write(assignedIndex);
            writer.Write(questIndex);
            writer.Write(money);
            return ms.ToArray();
        }

        public static byte[] SerializePlayerTransform(Vector3 pos, float yaw, float pitch, float speed, bool isCrouching, int toolId, bool isInteracting)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.PlayerTransform);
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);
            writer.Write(yaw);
            writer.Write(pitch);
            writer.Write(speed);
            writer.Write(isCrouching);
            writer.Write(toolId);
            writer.Write(isInteracting);
            return ms.ToArray();
        }

        public static byte[] SerializeQuestSync(int activeIndex, long[] progress, bool[] done)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.QuestSync);
            writer.Write(activeIndex);
            writer.Write(progress != null ? progress.Length : 0);
            if (progress != null)
            {
                for (int i = 0; i < progress.Length; i++)
                    writer.Write(progress[i]);
            }
            writer.Write(done != null ? done.Length : 0);
            if (done != null)
            {
                for (int i = 0; i < done.Length; i++)
                    writer.Write(done[i]);
            }
            return ms.ToArray();
        }

        public static byte[] SerializeVoxelCut(Vector3 localPoint, float radius, float cutDelta)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.VoxelCut);
            writer.Write(localPoint.x);
            writer.Write(localPoint.y);
            writer.Write(localPoint.z);
            writer.Write(radius);
            writer.Write(cutDelta);
            return ms.ToArray();
        }

        public static byte[] SerializeMachineSync(int machineId, float crankAngle, bool isRunning, int peeledCount)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.MachineSync);
            writer.Write(machineId);
            writer.Write(crankAngle);
            writer.Write(isRunning);
            writer.Write(peeledCount);
            return ms.ToArray();
        }

        public static byte[] SerializeDoorSync(int doorId, bool isOpen, bool isSliding = false)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.DoorSync);
            writer.Write(doorId);
            writer.Write(isOpen);
            writer.Write(isSliding);
            return ms.ToArray();
        }

        public static byte[] SerializeEconomySync(long totalMoney)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.EconomySync);
            writer.Write(totalMoney);
            return ms.ToArray();
        }

        public static byte[] SerializeUpgradeSync(int upgradeId, int level)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.UpgradeSync);
            writer.Write(upgradeId);
            writer.Write(level);
            return ms.ToArray();
        }

        public static byte[] SerializeSackRipped(int sackIndex)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.SackRipped);
            writer.Write(sackIndex);
            return ms.ToArray();
        }

        public static byte[] SerializePotatoClean(int workstationIndex)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.PotatoClean);
            writer.Write(workstationIndex);
            return ms.ToArray();
        }

        public static byte[] SerializeQuestBump(int kind, long amount)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.QuestBump);
            writer.Write(kind);
            writer.Write(amount);
            return ms.ToArray();
        }

        public static byte[] SerializeQuestFlag(string flagId)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.QuestFlag);
            writer.Write(flagId ?? "");
            return ms.ToArray();
        }

        public static byte[] SerializeQuestPage(int pageIndex)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write((byte)PacketType.QuestPage);
            writer.Write(pageIndex);
            return ms.ToArray();
        }
    }
}
