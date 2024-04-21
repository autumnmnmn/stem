using System.Net.Sockets;
using System.Text;

namespace Sandbox.ScratchPad;

public class SocketListener<T> where T : struct, Enum {
    private T ExitCode { get; init; }

    private string SocketName { get; init; }

    private Dictionary<T, Action<int, byte[]>> CodeHandlers { get; init; } = new();

    public CancellationTokenSource? CancellationTokenSource { get; private set; }

    public void Cancel() => CancellationTokenSource?.Cancel();

    public void Start() {
        if (CancellationTokenSource is not null) throw new InvalidOperationException("Cannot start the same Listener twice.");

        CancellationTokenSource = new CancellationTokenSource();

        new Thread(new ParameterizedThreadStart(ListenerThread)).Start(CancellationTokenSource.Token);
    }

    public SocketListener(T exitCode, string socketName) {
        ExitCode = exitCode;
        SocketName = socketName;
    }

    public void On(T code, Action<int, byte[]> handler) {
        CodeHandlers[code] = handler;
    }

    public void ListenerThread(object? cancellationTokenObject) 
    {
        if (cancellationTokenObject is not CancellationToken cancellationToken) 
            throw new InvalidProgramException("Listener thread must have a cancellation token.");
        
        do
        {
            try 
            {
                Listen(cancellationToken);
            }
            catch (Exception e) 
            {
                Console.WriteLine($"[Listener] Unhandled Exception: {e}");
            }
            Thread.Sleep(1000);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    public void Listen(CancellationToken cancellationToken) 
    {
        using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
        socket.Connect(new UnixDomainSocketEndPoint(SocketName));
        var codeBuffer = new byte[4];
        var contentLengthBuffer = new byte[4];
        var shouldExit = false;
        while (socket.Connected && !shouldExit && !cancellationToken.IsCancellationRequested) {
            socket.Receive(codeBuffer);

            var code = (T)Enum.ToObject(typeof(T), BitConverter.ToInt32(codeBuffer, 0));

            if (code.Equals(ExitCode)) {
                shouldExit = true;
                continue;
            }

            socket.Receive(contentLengthBuffer);

            var contentSize = BitConverter.ToInt32(contentLengthBuffer, 0);

            var contentBuffer = new byte[contentSize];

            var receivedBytesTotal = 0;
            var receivedBytes = 0;
            do
            {
                receivedBytes = socket.Receive(contentBuffer, receivedBytesTotal, contentSize - receivedBytesTotal, SocketFlags.None);
                receivedBytesTotal += receivedBytes;
            } 
            while (receivedBytesTotal < contentSize && receivedBytes > 0);

            if (receivedBytesTotal < contentSize) {
                Console.WriteLine($"[Listener] Only received {receivedBytesTotal} of {contentSize} bytes for a {code} transmission.");
                continue;
            }

            if (!CodeHandlers.ContainsKey(code)) {
                Console.WriteLine($"[Listener] No handler for code {code}. Disregarding {contentSize} bytes of data starting with \n[{string.Join(' ', contentBuffer.Take(64).Select(b => b.ToString()))}]");
                continue;
            }
            
            CodeHandlers[code](contentSize, contentBuffer);
        }

        socket.Disconnect(false);
        socket.Close();
    }
}
