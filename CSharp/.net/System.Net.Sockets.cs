using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace __System.Net.Sockets
{
    public enum AddressFamily
    {
        Unknown = -1,
        Unspecified,
        Unix,
        InterNetwork,
        ImpLink,
        Pup,
        Chaos,
        NS,
        Ipx = 6,
        Iso,
        Osi = 7,
        Ecma,
        DataKit,
        Ccitt,
        Sna,
        DecNet,
        DataLink,
        Lat,
        HyperChannel,
        AppleTalk,
        NetBios,
        VoiceView,
        FireFox,
        Banyan = 21,
        Atm,
        InterNetworkV6,
        Cluster,
        Ieee12844,
        Irda,
        NetworkDesigners = 28,
        Max
    }
    public enum SocketError
    {
        Success,
        SocketError = -1,
        Interrupted = 10004,
        AccessDenied = 10013,
        Fault,
        InvalidArgument = 10022,
        TooManyOpenSockets = 10024,
        WouldBlock = 10035,
        InProgress,
        AlreadyInProgress,
        NotSocket,
        DestinationAddressRequired,
        MessageSize,
        ProtocolType,
        ProtocolOption,
        ProtocolNotSupported,
        SocketNotSupported,
        OperationNotSupported,
        ProtocolFamilyNotSupported,
        AddressFamilyNotSupported,
        AddressAlreadyInUse,
        AddressNotAvailable,
        NetworkDown,
        NetworkUnreachable,
        NetworkReset,
        ConnectionAborted,
        ConnectionReset,
        NoBufferSpaceAvailable,
        IsConnected,
        NotConnected,
        Shutdown,
        TimedOut = 10060,
        ConnectionRefused,
        HostDown = 10064,
        HostUnreachable,
        ProcessLimit = 10067,
        SystemNotReady = 10091,
        VersionNotSupported,
        NotInitialized,
        Disconnecting = 10101,
        TypeNotFound = 10109,
        HostNotFound = 11001,
        TryAgain,
        NoRecovery,
        NoData,
        IOPending = 997,
        OperationAborted = 995
    }
    public class SocketException : Exception
    {
        private EndPoint m_EndPoint;
        private int errorCode;
        public int ErrorCode
        {
            get
            {
                return errorCode;
            }
        }
        public override string Message
        {
            get
            {
                if (this.m_EndPoint == null)
                {
                    return base.Message;
                }
                return base.Message + " " + this.m_EndPoint.ToString();
            }
        }
        public SocketError SocketErrorCode
        {
            get
            {
                return (SocketError)errorCode;
            }
        }
        public SocketException()
        {
        }
        internal SocketException(EndPoint endPoint)
        {
            this.m_EndPoint = endPoint;
        }
        public SocketException(int errorCode)
            : this(errorCode, null)
        {
        }
        internal SocketException(int errorCode, EndPoint endPoint)
        {
            this.errorCode = errorCode;
            this.m_EndPoint = endPoint;
        }
        internal SocketException(SocketError socketError)
            : this((int)socketError)
        {
        }
    }
}
