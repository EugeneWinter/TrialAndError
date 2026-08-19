/*using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

public static class LightEngine
{
    public const int MAX_LIGHT = 15;
    public const int CHUNK_SIZE = 32;
    public const int MAX_SUN_RAY_STEPS = 48;

    public static byte[] CalculateLight(ChunkData chunk, WorldManager world, Vector3 sunDirection)
    {
        byte[] lightMap = new byte[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        Queue<int3> propagationQueue = new Queue<int3>();

        int3 coord = chunk.position;
        Vector3 sunDirNorm = -sunDirection.normalized;

        for (int x = 0; x < CHUNK_SIZE; x++)
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                int wx = coord.x * CHUNK_SIZE + x;
                int wz = coord.z * CHUNK_SIZE + z;

                bool sunBlockedVertically = false;
                int topChunkY = coord.y + 1;
                int3 aboveCoord = new int3(coord.x, topChunkY, coord.z);

                if (world.HasChunk(aboveCoord))
                {
                    ChunkData aboveChunk = world.GetChunkData(aboveCoord);
                    for (int checkY = 0; checkY < CHUNK_SIZE; checkY++)
                    {
                        ushort block = aboveChunk.GetBlock(x, checkY, z);
                        if (block != 0 && IsOpaque(block))
                        {
                            sunBlockedVertically = true;
                            break;
                        }
                    }
                }

                bool firstOpenCellChecked = false;

                for (int y = CHUNK_SIZE - 1; y >= 0; y--)
                {
                    ushort block = chunk.GetBlock(x, y, z);

                    if (block != 0 && IsOpaque(block))
                    {
                        sunBlockedVertically = true;
                        continue;
                    }

                    if (!sunBlockedVertically)
                    {
                        int idx = x + y * CHUNK_SIZE + z * CHUNK_SIZE * CHUNK_SIZE;
                        lightMap[idx] = MAX_LIGHT;
                        propagationQueue.Enqueue(new int3(x, y, z));

                        if (!firstOpenCellChecked)
                        {
                            firstOpenCellChecked = true;
                            int wy = coord.y * CHUNK_SIZE + y;
                            bool sunReaches = TraceRayToSun(wx, wy, wz, sunDirNorm, world);

                            if (!sunReaches)
                            {
                                lightMap[idx] = (byte)(MAX_LIGHT / 2);
                            }
                        }
                    }
                }
            }

        while (propagationQueue.Count > 0)
        {
            int3 pos = propagationQueue.Dequeue();
            int idx = pos.x + pos.y * CHUNK_SIZE + pos.z * CHUNK_SIZE * CHUNK_SIZE;
            byte currentLight = lightMap[idx];

            if (currentLight <= 1) continue;

            byte spread = (byte)(currentLight - 1);

            TrySpread(lightMap, propagationQueue, chunk, world, pos.x - 1, pos.y, pos.z, spread);
            TrySpread(lightMap, propagationQueue, chunk, world, pos.x + 1, pos.y, pos.z, spread);
            TrySpread(lightMap, propagationQueue, chunk, world, pos.x, pos.y - 1, pos.z, spread);
            TrySpread(lightMap, propagationQueue, chunk, world, pos.x, pos.y + 1, pos.z, spread);
            TrySpread(lightMap, propagationQueue, chunk, world, pos.x, pos.y, pos.z - 1, spread);
            TrySpread(lightMap, propagationQueue, chunk, world, pos.x, pos.y, pos.z + 1, spread);
        }

        return lightMap;
    }

    static bool TraceRayToSun(int startX, int startY, int startZ, Vector3 sunDir, WorldManager world)
    {
        float x = startX + 0.5f;
        float y = startY + 0.5f;
        float z = startZ + 0.5f;

        int stepX = sunDir.x >= 0 ? 1 : -1;
        int stepY = sunDir.y >= 0 ? 1 : -1;
        int stepZ = sunDir.z >= 0 ? 1 : -1;

        float tDeltaX = sunDir.x != 0 ? Mathf.Abs(1f / sunDir.x) : float.MaxValue;
        float tDeltaY = sunDir.y != 0 ? Mathf.Abs(1f / sunDir.y) : float.MaxValue;
        float tDeltaZ = sunDir.z != 0 ? Mathf.Abs(1f / sunDir.z) : float.MaxValue;

        int currentX = startX;
        int currentY = startY;
        int currentZ = startZ;

        float tMaxX = sunDir.x != 0 ? ((stepX > 0 ? (currentX + 1) : currentX) - x) / sunDir.x : float.MaxValue;
        float tMaxY = sunDir.y != 0 ? ((stepY > 0 ? (currentY + 1) : currentY) - y) / sunDir.y : float.MaxValue;
        float tMaxZ = sunDir.z != 0 ? ((stepZ > 0 ? (currentZ + 1) : currentZ) - z) / sunDir.z : float.MaxValue;

        for (int i = 0; i < MAX_SUN_RAY_STEPS; i++)
        {
            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                currentX += stepX;
                tMaxX += tDeltaX;
            }
            else if (tMaxY < tMaxZ)
            {
                currentY += stepY;
                tMaxY += tDeltaY;
            }
            else
            {
                currentZ += stepZ;
                tMaxZ += tDeltaZ;
            }

            if (currentY > 200) return true;

            ushort block = world.GetBlock(currentX, currentY, currentZ);
            if (block != 0 && IsOpaque(block))
                return false;
        }

        return true;
    }

    static void TrySpread(byte[] lightMap, Queue<int3> queue, ChunkData chunk, WorldManager world, int x, int y, int z, byte lightLevel)
    {
        if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_SIZE || z < 0 || z >= CHUNK_SIZE)
            return;

        ushort block = chunk.GetBlock(x, y, z);
        if (block != 0 && IsOpaque(block))
            return;

        int idx = x + y * CHUNK_SIZE + z * CHUNK_SIZE * CHUNK_SIZE;

        if (lightMap[idx] >= lightLevel)
            return;

        lightMap[idx] = lightLevel;
        queue.Enqueue(new int3(x, y, z));
    }

    static bool IsOpaque(ushort blockId)
    {
        if (blockId == BlockIDs.Air) return false;
        if (blockId == BlockIDs.Water) return false;
        if (blockId == BlockIDs.Leaf) return false;
        return true;
    }
}*/