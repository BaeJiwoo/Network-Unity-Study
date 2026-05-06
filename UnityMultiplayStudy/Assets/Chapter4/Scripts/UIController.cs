using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public TMP_InputField mUserInputField;
    public TransportTCP mTransportTCP;
    public ushort mPort;
    
    public void JoinRoom()
    {
        mTransportTCP.Connect(mUserInputField.text, mPort);

    }
    public void StartServer()
    {
        mTransportTCP.StartServer(mPort, 2);
    }
}
