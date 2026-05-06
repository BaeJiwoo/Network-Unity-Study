using UnityEngine;

using System.Text;
public class Chat : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (mChatState)
        {
            case ChatState.CHATTING:
                UpdateChatting();
                break;
            case ChatState.LEAVE:
                UpdateLeave();
                break;
        }
    }

    private void UpdateChatting()
    {
        byte[] buffer = new byte[1400];

        int recvSize = mTransportTCP.Receive(ref buffer, buffer.Length);
        if(recvSize > 0)
        {
            string message = System.Text.Encoding.UTF8.GetString(buffer);

            int id = (mIsServer == true) ? 1 : 0;
            //AddMessage(ref mMessage[id], message);
        }
    }

    private void UpdateLeave()
    {

    }

    private void AddMessage()
    {

    }

    private ChatState mChatState;

    public TransportTCP mTransportTCP;

    public bool mIsServer = false;

}
