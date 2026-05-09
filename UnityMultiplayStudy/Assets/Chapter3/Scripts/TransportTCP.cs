using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections;
using System.Threading;


public class TransportTCP : MonoBehaviour
{
    public void Start()
    {
        mSendQueue = new PacketQueue();
        mRecvQueue = new PacketQueue();
    }

    public void FixedUpdate()
    {

    }

    public bool StartServer(int port, int connectionNum)
    {
        Debug.Log("StartServer called.!");
        try
        {
            mListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            mListener.Bind(new IPEndPoint(IPAddress.Any, port));
            mListener.Listen(connectionNum);
        }
        catch
        {
            Debug.Log("StartServer fail");
            return false;
        }
        mIsServer = true;
        return LaunchThread();
    }
    public void HiHi() => Debug.Log("Hi");
    public void StopServer()
    {
        mIsThreadRunning = false;
        if (mThread != null)
        {
            mThread.Join();
            mThread = null;
        }

        DisConnect();

        if (mListener != null)
        {
            mListener.Close();
            mListener = null;
        }

        mIsServer = false;

        Debug.Log("Server stopped.");
    }
    public bool Connect(string address, int port)
    {
        Debug.Log("TransportTCP connect called.");

        if (mListener != null) return false;

        bool ret = false;
        try
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.NoDelay = true;
            Debug.Log($"Connecting...{address}{port}");
            mSocket.Connect(address, port);
            Debug.Log("LaunchThread...");
            ret = LaunchThread();
        }
        catch
        {
            mSocket = null;
        }

        if (ret == true)
        {
            mIsConnected = true;
            Debug.Log("Connection Successed");
        }
        else
        {
            mIsConnected = false;
            Debug.Log("Connection Fail");
        }

        if (mHandler != null)
        {
            NetEventState state = new NetEventState();
            state.type = NetEventType.Connect;
            state.result = (mIsConnected == true) ? NetEventResult.Success : NetEventResult.Failure;
            mHandler(state);
            Debug.Log("event handler called");
        }

        return mIsConnected;
    }

    public bool DisConnect()
    {
        mIsConnected = false;

        if (mSocket != null)
        {
            mSocket.Shutdown(SocketShutdown.Both);
            mSocket.Close();
            mSocket = null;
        }



        if (mHandler != null)
        {
            NetEventState state = new NetEventState();
            state.type = NetEventType.DisConnect;
            state.result = NetEventResult.Success;
            mHandler(state);
        }

        return true;
    }

    public int Send(byte[] data, int size)
    {
        if (mSendQueue == null) return 0;
        return mSendQueue.Enqueue(data, size);
    }

    public int Receive(ref byte[] data, int size)
    {
        if (mRecvQueue == null) return 0;

        //mUIController.GetPacket();
        return mRecvQueue.Dequeue(ref data, size);
    }

    public void RegisterEventHander(EventHandler handler)
    {
        mHandler += handler;
    }

    public void UnregisterEventHander(EventHandler handler)
    {
        mHandler -= handler;
    }

    bool LaunchThread()
    {
        try
        {
            mIsThreadRunning = true;
            mThread = new Thread(new ThreadStart(Dispatch));
            mThread.Start();
        }
        catch
        {
            Debug.LogError("Cannot launch thread.");
            return false;
        }
        return true;
    }

    public void Dispatch()
    {
        Debug.Log("Dispatch thread started...");

        while (mIsThreadRunning)
        {
            //Debug.Log("Dispatch thread Running...");
            AcceptClient();

            if (mSocket != null && mIsConnected == true)
            {
                DisPatchSend();

                DisPatchReceive();
            }

            Thread.Sleep(5);
        }
    }

    void AcceptClient()
    {
        if (mListener != null && mListener.Poll(0, SelectMode.SelectRead))
        {
            mSocket = mListener.Accept();
            mIsConnected = true;
            Debug.Log("Connected from client");
        }
    }

    void DisPatchSend()
    {
        try
        {
            if (mSocket.Poll(0, SelectMode.SelectWrite))
            {
                byte[] buffer = new byte[s_mtu];

                int sendSize = mSendQueue.Dequeue(ref buffer, buffer.Length);
                Debug.Log(sendSize);
                while (sendSize > 0)
                {

                    mSocket.Send(buffer, sendSize, SocketFlags.None);
                    sendSize = mSendQueue.Dequeue(ref buffer, buffer.Length);
                }
            }
        }
        catch
        {
            return;
        }
    }

    void DisPatchReceive()
    {
        try
        {
            if (mSocket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[s_mtu];

                int recvSize = mSocket.Receive(buffer, SocketFlags.None);
                if (recvSize == 0)
                {
                    Debug.Log("Disconnect recv from client");
                    DisConnect();
                }
                else if (recvSize > 0)
                {
                    mRecvQueue.Enqueue(buffer, recvSize);
                }
            }
        }
        catch
        {
            return;
        }
    }

    // 서버인지 확인.
    public bool IsServer()
    {
        return mIsServer;
    }

    // 접속확인.
    public bool IsConnected()
    {
        return mIsConnected;
    }

    // properties

    private Socket mListener;

    private Socket mSocket;

    private bool mIsServer = false;

    private bool mIsConnected = false;

    private PacketQueue mSendQueue;

    private PacketQueue mRecvQueue;

    private EventHandler mHandler;

    public delegate void EventHandler(NetEventState state);

    // thread 관련 함수
    private bool mIsThreadRunning = false;

    private Thread mThread;

    // 버퍼
    private static int s_mtu = 1400;

    public UIController mUIController;
}
