/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.    
*/

namespace Myrtille.Web
{
    public enum RemoteSessionState
    {
        NotConnected = 0, 
        Connecting = 1,
        Connected = 2,
        Disconnecting = 3,
        Disconnected = 4
    }
}