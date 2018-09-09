using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    public interface ISocket
    {
        nng_socket NngSocket { get; }
        void SetOpt(string name, byte[] data);
    }

    public interface IReqSocket : ISocket
    {
    }

    public interface IRepSocket : ISocket
    {
    }

    public interface IPushSocket : ISocket
    {
    }

    public interface IPullSocket : ISocket
    {
    }

    public interface IPubSocket : ISocket
    {
    }

    public interface ISubscribeSocket : ISocket
    {
    }
}