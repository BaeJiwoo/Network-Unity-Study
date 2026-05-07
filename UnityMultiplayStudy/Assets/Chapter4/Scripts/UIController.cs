using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class UIController : MonoBehaviour
{
    public TMP_InputField mHostInputField;
    public TMP_InputField mUserTextInputField;
    public TransportTCP mTransportTCP;
    public ushort mPort;

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
        mUserTextInputField.text = "";
    }
}
