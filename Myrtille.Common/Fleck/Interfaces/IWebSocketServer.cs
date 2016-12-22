using System;

namespace Myrtille.Fleck
{
    public interface IWebSocketServer : IDisposable
    {
        void Start(Action<IWebSocketConnection> config);
    }
}
