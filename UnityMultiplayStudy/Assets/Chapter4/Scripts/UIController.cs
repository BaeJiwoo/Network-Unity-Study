using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;

public class UIController : MonoBehaviour
{
    private ChatState mChatState;

    public TMP_InputField mHostInputField;
    public TMP_InputField mUserTextInputField;
    public TransportTCP mTransportTCP;
    public ushort mPort;

    public RectTransform RoomManagerChatContentPos;
    public RectTransform ClientChatContentPos;

    public GameObject messagePrefab;

    private void FixedUpdate()
    {
        UpdateChatting();
    }


    public void JoinRoom()
    {
        mTransportTCP.Connect(mHostInputField.text, mPort);

    }
    public void CreateRoom()
    {
        mTransportTCP.StartServer(mPort, 2);
    }

    public void SendMessage()
    {
        byte[] message = Encoding.UTF8.GetBytes(mUserTextInputField.text);
        mTransportTCP.Send(message, message.Length);
        DateTime now = DateTime.Now;
        AddMessage($"{mUserTextInputField.text}::{now.ToString()}", true);
        mUserTextInputField.text = "";
    }

    void UpdateChatting()
    {
        byte[] buffer = new byte[1400];

        int recvSize = mTransportTCP.Receive(ref buffer, buffer.Length);
        if (recvSize > 0)
        {
            DateTime now = DateTime.Now;
            string message = System.Text.Encoding.UTF8.GetString(buffer);
            //Debug.Log("Recv data:" + message + "::" + now);

            AddMessage($"{message}::{now.ToString()}", false);
        }
    }

    void AddMessage(string message, bool isMine)
    {
        // 서버면 1, 아니면(클라이언트면) 2를 할당
        //int id = (mTransportTCP.IsServer() == true) ? 1 : 0;

        switch (isMine)
        {
            case false: // 클라이언트용 로직 (예: 오른쪽 정렬 메시지 박스)
                SpawnMessage(message, ClientChatContentPos);
                break;
            case true: // 서버용 로직 (예: 왼쪽 정렬 메시지 박스)
                SpawnMessage(message, RoomManagerChatContentPos);
                break;

        }
    }

    void SpawnMessage(string message, RectTransform parent)
    {
        // 부모(parent)를 지정하며 생성, worldPositionStays는 false로 설정하여 UI 좌표 유지
        GameObject go = Instantiate(messagePrefab, parent, false);

        // 텍스트 컴포넌트 설정 (TextMeshPro 또는 기본 Text)
        var textComp = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComp != null) textComp.text = message;
    }
}
