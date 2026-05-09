
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class PacketQueue
{
    public PacketQueue()
    {
        mStreamBuffer = new MemoryStream();
        mOffsetList = new List<PacketInfo>();
    }

    public int Enqueue(byte[] data, int size)
    {
        UnityEngine.Debug.Log($"Enqueued {System.Text.Encoding.UTF8.GetString(data, 0, size)}{size}");
        PacketInfo info = new PacketInfo();
        info.offset = mOffset;
        info.size = size;

        lock (lockObj)
        {
            mOffsetList.Add(info);

            mStreamBuffer.Position = mOffset;
            mStreamBuffer.Write(data, 0, size);
            mStreamBuffer.Flush();
            mOffset += size;
        }
        return size;
    }

    public int Dequeue(ref byte[] buffer, int size)
    {
        if (mOffsetList.Count <= 0) return -1;

        int recvSize = 0;
        lock (lockObj)
        {
            PacketInfo info = mOffsetList[0];

            int dataSize = Math.Min(size, info.size);
            mStreamBuffer.Position = info.offset;
            recvSize = mStreamBuffer.Read(buffer, 0, dataSize);

            if(recvSize > 0)
            {
                mOffsetList.RemoveAt(0);
            }

            if(mOffsetList.Count == 0)
            {
                Clear();
                mOffset = 0;
            }
        }
        return recvSize;
    }

    public bool IsEmpty() => mOffsetList.Count > 0;

    public void Clear()
    {
        byte[] buffer = mStreamBuffer.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);

        mStreamBuffer.Position = 0;
        mStreamBuffer.SetLength(0);
    }

    private MemoryStream mStreamBuffer;

    private List<PacketInfo> mOffsetList;

    private int mOffset = 0;

    private Object lockObj = new Object();
}


