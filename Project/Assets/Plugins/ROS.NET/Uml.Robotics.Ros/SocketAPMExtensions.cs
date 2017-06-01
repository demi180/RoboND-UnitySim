using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ROS_Comm.APMWorkaround
{
    // Workaround suggested by: https://github.com/dotnet/corefx/issues/10566#issuecomment-237634035
    // SOURCE: https://raw.githubusercontent.com/dotnet/corefx/d0dc5fc099946adc1035b34a8b1f6042eddb0c75/src/Common/src/System/Net/Sockets/SocketAPMExtensions.cs

    internal static class SocketAPMExtensions
    {
        //
        // Summary:
        //     Begins an asynchronous operation to accept an incoming connection attempt.
        //
        // Parameters:
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object that contains state information for this request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous System.Net.Sockets.Socket
        //     creation.
        //
        // Exceptions:
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket object has been closed.
        //
        //   T:System.NotSupportedException:
        //     Windows NT is required for this method.
        //
        //   T:System.InvalidOperationException:
        //     The accepting socket is not listening for connections. You must call System.Net.Sockets.Socket.Bind(System.Net.EndPoint)
        //     and System.Net.Sockets.Socket.Listen(System.Int32) before calling System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object).-or-
        //     The accepted socket is bound.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     receiveSize is less than 0.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        public static IAsyncResult BeginAccept(this Socket socket, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(socket.AcceptAsync(), callback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection.
        //
        // Parameters:
        //   remoteEP:
        //     An System.Net.EndPoint that represents the remote host.
        //
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object that contains state information for this request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     remoteEP is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.Socket is System.Net.Sockets.Socket.Listen(System.Int32)ing.
        public static IAsyncResult BeginConnect(this Socket socket, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(socket.ConnectAsync(remoteEP), callback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The host is specified
        //     by an System.Net.IPAddress array and a port number.
        //
        // Parameters:
        //   addresses:
        //     At least one System.Net.IPAddress, designating the remote host.
        //
        //   port:
        //     The port number of the remote host.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     connect operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous connections.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     addresses is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.NotSupportedException:
        //     This method is valid for sockets that use System.Net.Sockets.AddressFamily.InterNetwork
        //     or System.Net.Sockets.AddressFamily.InterNetworkV6.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        //
        //   T:System.ArgumentException:
        //     The length of address is zero.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.Socket is System.Net.Sockets.Socket.Listen(System.Int32)ing.
        public static IAsyncResult BeginConnect(this Socket socket, IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(socket.ConnectAsync(addresses, port), requestCallback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The host is specified
        //     by an System.Net.IPAddress and a port number.
        //
        // Parameters:
        //   address:
        //     The System.Net.IPAddress of the remote host.
        //
        //   port:
        //     The port number of the remote host.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     connect operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     address is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.NotSupportedException:
        //     The System.Net.Sockets.Socket is not in the socket family.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        //
        //   T:System.ArgumentException:
        //     The length of address is zero.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.Socket is System.Net.Sockets.Socket.Listen(System.Int32)ing.
        public static IAsyncResult BeginConnect(this Socket socket, IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(socket.ConnectAsync(address, port), requestCallback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The host is specified
        //     by a host name and a port number.
        //
        // Parameters:
        //   host:
        //     The name of the remote host.
        //
        //   port:
        //     The port number of the remote host.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     connect operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     host is null.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.NotSupportedException:
        //     This method is valid for sockets in the System.Net.Sockets.AddressFamily.InterNetwork
        //     or System.Net.Sockets.AddressFamily.InterNetworkV6 families.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.Socket is System.Net.Sockets.Socket.Listen(System.Int32)ing.
        public static IAsyncResult BeginConnect(this Socket socket, string host, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(socket.ConnectAsync(host, port), requestCallback, state);
        }

        //
        // Summary:
        //     Begins to asynchronously receive data from a connected System.Net.Sockets.Socket.
        //
        // Parameters:
        //   buffers:
        //     An array of type System.Byte that is the storage location for the received data.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values.
        //
        //   callback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the receive operation.
        //     This object is passed to the System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)
        //     delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous read.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     System.Net.Sockets.Socket has been closed.
        public static IAsyncResult BeginReceive(this Socket socket, IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(socket.ReceiveAsync(buffers, socketFlags), callback, state);
        }

        //
        // Summary:
        //     Begins to asynchronously receive data from a connected System.Net.Sockets.Socket.
        //
        // Parameters:
        //   buffer:
        //     An array of type System.Byte that is the storage location for the received data.
        //
        //   offset:
        //     The zero-based position in the buffer parameter at which to store the received
        //     data.
        //
        //   size:
        //     The number of bytes to receive.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values.
        //
        //   callback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the receive operation.
        //     This object is passed to the System.Net.Sockets.Socket.EndReceive(System.IAsyncResult)
        //     delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous read.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     System.Net.Sockets.Socket has been closed.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     offset is less than 0.-or- offset is greater than the length of buffer.-or- size
        //     is less than 0.-or- size is greater than the length of buffer minus the value
        //     of the offset parameter.
        public static IAsyncResult BeginReceive(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(socket.ReceiveAsync(CreateArraySegment(buffer, offset, size), socketFlags), callback, state);
        }

        //
        // Summary:
        //     Begins to asynchronously receive data from a specified network device.
        //
        // Parameters:
        //   buffer:
        //     An array of type System.Byte that is the storage location for the received data.
        //
        //   offset:
        //     The zero-based position in the buffer parameter at which to store the data.
        //
        //   size:
        //     The number of bytes to receive.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values.
        //
        //   remoteEP:
        //     An System.Net.EndPoint that represents the source of the data.
        //
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object that contains state information for this request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous read.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.-or- remoteEP is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     offset is less than 0.-or- offset is greater than the length of buffer.-or- size
        //     is less than 0.-or- size is greater than the length of buffer minus the value
        //     of the offset parameter.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        public static IAsyncResult BeginReceiveFrom(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            // remoteEP will not change in the sync portion.
            return TaskToApm.Begin(socket.ReceiveFromAsync(CreateArraySegment(buffer, offset, size), socketFlags, remoteEP), callback, state);
        }

        //
        // Summary:
        //     Begins to asynchronously receive the specified number of bytes of data into the
        //     specified location of the data buffer, using the specified System.Net.Sockets.SocketFlags,
        //     and stores the endpoint and packet information..
        //
        // Parameters:
        //   buffer:
        //     An array of type System.Byte that is the storage location for the received data.
        //
        //   offset:
        //     The zero-based position in the buffer parameter at which to store the data.
        //
        //   size:
        //     The number of bytes to receive.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values.
        //
        //   remoteEP:
        //     An System.Net.EndPoint that represents the source of the data.
        //
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object that contains state information for this request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous read.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.-or- remoteEP is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     offset is less than 0.-or- offset is greater than the length of buffer.-or- size
        //     is less than 0.-or- size is greater than the length of buffer minus the value
        //     of the offset parameter.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.NotSupportedException:
        //     The operating system is Windows 2000 or earlier, and this method requires Windows
        //     XP.
        public static IAsyncResult BeginReceiveMessageFrom(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            // remoteEP will not change in the sync portion.
            return TaskToApm.Begin(socket.ReceiveMessageFromAsync(CreateArraySegment(buffer, offset, size), socketFlags, remoteEP), callback, state);
        }

        //
        // Summary:
        //     Sends data asynchronously to a connected System.Net.Sockets.Socket.
        //
        // Parameters:
        //   buffers:
        //     An array of type System.Byte that contains the data to send.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values.
        //
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object that contains state information for this request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous send.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffers is null.
        //
        //   T:System.ArgumentException:
        //     buffers is empty.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See remarks section below.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static IAsyncResult BeginSend(this Socket socket, IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(socket.SendAsync(buffers, socketFlags), callback, state);
        }

        //
        // Summary:
        //     Sends data asynchronously to a connected System.Net.Sockets.Socket.
        //
        // Parameters:
        //   buffer:
        //     An array of type System.Byte that contains the data to send.
        //
        //   offset:
        //     The zero-based position in the buffer parameter at which to begin sending data.
        //
        //   size:
        //     The number of bytes to send.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values.
        //
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object that contains state information for this request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous send.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See remarks section below.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     offset is less than 0.-or- offset is less than the length of buffer.-or- size
        //     is less than 0.-or- size is greater than the length of buffer minus the value
        //     of the offset parameter.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static IAsyncResult BeginSend(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(socket.SendAsync(CreateArraySegment(buffer, offset, size), socketFlags), callback, state);
        }

        //
        // Summary:
        //     Sends data asynchronously to a specific remote host.
        //
        // Parameters:
        //   buffer:
        //     An array of type System.Byte that contains the data to send.
        //
        //   offset:
        //     The zero-based position in buffer at which to begin sending data.
        //
        //   size:
        //     The number of bytes to send.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values.
        //
        //   remoteEP:
        //     An System.Net.EndPoint that represents the remote device.
        //
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object that contains state information for this request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous send.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.-or- remoteEP is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     offset is less than 0.-or- offset is greater than the length of buffer.-or- size
        //     is less than 0.-or- size is greater than the length of buffer minus the value
        //     of the offset parameter.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        public static IAsyncResult BeginSendTo(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(socket.SendToAsync(CreateArraySegment(buffer, offset, size), socketFlags, remoteEP), callback, state);
        }

        //
        // Summary:
        //     Asynchronously accepts an incoming connection attempt and creates a new System.Net.Sockets.Socket
        //     to handle remote host communication.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that stores state information for this asynchronous operation
        //     as well as any user defined data.
        //
        // Returns:
        //     A System.Net.Sockets.Socket to handle communication with the remote host.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not created by a call to System.Net.Sockets.Socket.BeginAccept(System.AsyncCallback,System.Object).
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndAccept(System.IAsyncResult) method was previously
        //     called.
        //
        //   T:System.NotSupportedException:
        //     Windows NT is required for this method.
        public static Socket EndAccept(this Socket socket, IAsyncResult asyncResult)
        {
            return TaskToApm.End<Socket>(asyncResult);
        }

        //
        // Summary:
        //     Ends a pending asynchronous connection request.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that stores state information and any user defined data
        //     for this asynchronous operation.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.Socket.BeginConnect(System.Net.EndPoint,System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndConnect(System.IAsyncResult) was previously called
        //     for the asynchronous connection.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static void EndConnect(this Socket socket, IAsyncResult asyncResult)
        {
            TaskToApm.End(asyncResult);
        }

        //
        // Summary:
        //     Ends a pending asynchronous read.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that stores state information and any user defined data
        //     for this asynchronous operation.
        //
        // Returns:
        //     The number of bytes received.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.Socket.BeginReceive(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndReceive(System.IAsyncResult) was previously called
        //     for the asynchronous read.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static int EndReceive(this Socket socket, IAsyncResult asyncResult)
        {
            return TaskToApm.End<int>(asyncResult);
        }

        //
        // Summary:
        //     Ends a pending asynchronous read from a specific endpoint.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that stores state information and any user defined data
        //     for this asynchronous operation.
        //
        //   endPoint:
        //     The source System.Net.EndPoint.
        //
        // Returns:
        //     If successful, the number of bytes received. If unsuccessful, returns 0.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.Socket.BeginReceiveFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@,System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndReceiveFrom(System.IAsyncResult,System.Net.EndPoint@)
        //     was previously called for the asynchronous read.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static int EndReceiveFrom(this Socket socket, IAsyncResult asyncResult, ref EndPoint endPoint)
        {
            SocketReceiveFromResult result = TaskToApm.End<SocketReceiveFromResult>(asyncResult);
            endPoint = result.RemoteEndPoint;
            return result.ReceivedBytes;
        }

        //
        // Summary:
        //     Ends a pending asynchronous read from a specific endpoint. This method also reveals
        //     more information about the packet than System.Net.Sockets.Socket.EndReceiveFrom(System.IAsyncResult,System.Net.EndPoint@).
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that stores state information and any user defined data
        //     for this asynchronous operation.
        //
        //   socketFlags:
        //     A bitwise combination of the System.Net.Sockets.SocketFlags values for the received
        //     packet.
        //
        //   endPoint:
        //     The source System.Net.EndPoint.
        //
        //   ipPacketInformation:
        //     The System.Net.IPAddress and interface of the received packet.
        //
        // Returns:
        //     If successful, the number of bytes received. If unsuccessful, returns 0.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null-or- endPoint is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.Socket.BeginReceiveMessageFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@,System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndReceiveMessageFrom(System.IAsyncResult,System.Net.Sockets.SocketFlags@,System.Net.EndPoint@,System.Net.Sockets.IPPacketInformation@)
        //     was previously called for the asynchronous read.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static int EndReceiveMessageFrom(this Socket socket, IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
        {
            SocketReceiveMessageFromResult result = TaskToApm.End<SocketReceiveMessageFromResult>(asyncResult);
            socketFlags = result.SocketFlags;
            endPoint = result.RemoteEndPoint;
            ipPacketInformation = result.PacketInformation;
            return result.ReceivedBytes;
        }

        //
        // Summary:
        //     Ends a pending asynchronous send.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that stores state information for this asynchronous operation.
        //
        // Returns:
        //     If successful, the number of bytes sent to the System.Net.Sockets.Socket; otherwise,
        //     an invalid System.Net.Sockets.Socket error.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndSend(System.IAsyncResult) was previously called
        //     for the asynchronous send.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static int EndSend(this Socket socket, IAsyncResult asyncResult)
        {
            return TaskToApm.End<int>(asyncResult);
        }

        //
        // Summary:
        //     Ends a pending asynchronous send to a specific location.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that stores state information and any user defined data
        //     for this asynchronous operation.
        //
        // Returns:
        //     If successful, the number of bytes sent; otherwise, an invalid System.Net.Sockets.Socket
        //     error.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.Socket.BeginSendTo(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint,System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndSendTo(System.IAsyncResult) was previously called
        //     for the asynchronous send.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static int EndSendTo(this Socket socket, IAsyncResult asyncResult)
        {
            return TaskToApm.End<int>(asyncResult);
        }

        // Behavior adapter.
        private static ArraySegment<byte> CreateArraySegment(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            return new ArraySegment<byte>(buffer, offset, size);
        }
    }

    internal static class TcpClientAPMExtensions
    {
        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The remote host
        //     is specified by an System.Net.IPAddress array and a port number (System.Int32).
        //
        // Parameters:
        //   addresses:
        //     At least one System.Net.IPAddress that designates the remote hosts.
        //
        //   port:
        //     The port number of the remote hosts.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The addresses parameter is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        public static IAsyncResult BeginConnect(this TcpClient client, IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.ConnectAsync(addresses, port), requestCallback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The remote host
        //     is specified by an System.Net.IPAddress and a port number (System.Int32).
        //
        // Parameters:
        //   address:
        //     The System.Net.IPAddress of the remote host.
        //
        //   port:
        //     The port number of the remote host.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The address parameter is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        public static IAsyncResult BeginConnect(this TcpClient client, IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.ConnectAsync(address, port), requestCallback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The remote host
        //     is specified by a host name (System.String) and a port number (System.Int32).
        //
        // Parameters:
        //   host:
        //     The name of the remote host.
        //
        //   port:
        //     The port number of the remote host.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The host parameter is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        public static IAsyncResult BeginConnect(this TcpClient client, string host, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.ConnectAsync(host, port), requestCallback, state);
        }

        //
        // Summary:
        //     Ends a pending asynchronous connection attempt.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult object returned by a call to Overload:System.Net.Sockets.TcpClient.BeginConnect.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The asyncResult parameter is null.
        //
        //   T:System.ArgumentException:
        //     The asyncResult parameter was not returned by a call to a Overload:System.Net.Sockets.TcpClient.BeginConnect
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.TcpClient.EndConnect(System.IAsyncResult) method was previously
        //     called for the asynchronous connection.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the System.Net.Sockets.Socket. See
        //     the Remarks section for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The underlying System.Net.Sockets.Socket has been closed.
        public static void EndConnect(this TcpClient client, IAsyncResult asyncResult)
        {
            TaskToApm.End(asyncResult);
        }
    }

    internal static class TcpListenerAPMExtensions
    {
        //
        // Summary:
        //     Begins an asynchronous operation to accept an incoming connection attempt.
        //
        // Parameters:
        //   callback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object containing information about the accept operation. This
        //     object is passed to the callback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous creation of the System.Net.Sockets.Socket.
        //
        // Exceptions:
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred while attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static IAsyncResult BeginAcceptSocket(this TcpListener listener, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(listener.AcceptSocketAsync(), callback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous operation to accept an incoming connection attempt.
        //
        // Parameters:
        //   callback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object containing information about the accept operation. This
        //     object is passed to the callback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous creation of the System.Net.Sockets.TcpClient.
        //
        // Exceptions:
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred while attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static IAsyncResult BeginAcceptTcpClient(this TcpListener listener, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(listener.AcceptTcpClientAsync(), callback, state);
        }

        //
        // Summary:
        //     Asynchronously accepts an incoming connection attempt and creates a new System.Net.Sockets.Socket
        //     to handle remote host communication.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult returned by a call to the System.Net.Sockets.TcpListener.BeginAcceptSocket(System.AsyncCallback,System.Object)
        //     method.
        //
        // Returns:
        //     A System.Net.Sockets.Socket.The System.Net.Sockets.Socket used to send and receive
        //     data.
        //
        // Exceptions:
        //   T:System.ObjectDisposedException:
        //     The underlying System.Net.Sockets.Socket has been closed.
        //
        //   T:System.ArgumentNullException:
        //     The asyncResult parameter is null.
        //
        //   T:System.ArgumentException:
        //     The asyncResult parameter was not created by a call to the System.Net.Sockets.TcpListener.BeginAcceptSocket(System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.TcpListener.EndAcceptSocket(System.IAsyncResult) method
        //     was previously called.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred while attempting to access the System.Net.Sockets.Socket. See
        //     the Remarks section for more information.
        public static Socket EndAcceptSocket(this TcpListener listener, IAsyncResult asyncResult)
        {
            return TaskToApm.End<Socket>(asyncResult);
        }

        //
        // Summary:
        //     Asynchronously accepts an incoming connection attempt and creates a new System.Net.Sockets.TcpClient
        //     to handle remote host communication.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult returned by a call to the System.Net.Sockets.TcpListener.BeginAcceptTcpClient(System.AsyncCallback,System.Object)
        //     method.
        //
        // Returns:
        //     A System.Net.Sockets.TcpClient.The System.Net.Sockets.TcpClient used to send
        //     and receive data.
        public static TcpClient EndAcceptTcpClient(this TcpListener listener, IAsyncResult asyncResult)
        {
            return TaskToApm.End<TcpClient>(asyncResult);
        }
    }

    internal static class UdpClientAPMExtensions
    {
        //
        // Summary:
        //     Receives a datagram from a remote host asynchronously.
        //
        // Parameters:
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the receive operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous receive.
        public static IAsyncResult BeginReceive(this UdpClient client, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.ReceiveAsync(), requestCallback, state);
        }

        //
        // Summary:
        //     Sends a datagram to a destination asynchronously. The destination is specified
        //     by a System.Net.EndPoint.
        //
        // Parameters:
        //   datagram:
        //     A System.Byte array that contains the data to be sent.
        //
        //   bytes:
        //     The number of bytes to send.
        //
        //   endPoint:
        //     The System.Net.EndPoint that represents the destination for the data.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the send operation. This
        //     object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous send.
        public static IAsyncResult BeginSend(this UdpClient client, byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.SendAsync(datagram, bytes, endPoint), requestCallback, state);
        }

        //
        // Summary:
        //     Sends a datagram to a destination asynchronously. The destination is specified
        //     by the host name and port number.
        //
        // Parameters:
        //   datagram:
        //     A System.Byte array that contains the data to be sent.
        //
        //   bytes:
        //     The number of bytes to send.
        //
        //   hostname:
        //     The destination host.
        //
        //   port:
        //     The destination port number.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the send operation. This
        //     object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous send.
        public static IAsyncResult BeginSend(this UdpClient client, byte[] datagram, int bytes, string hostname, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.SendAsync(datagram, bytes, hostname, port), requestCallback, state);
        }

        //
        // Summary:
        //     Ends a pending asynchronous receive.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult object returned by a call to System.Net.Sockets.UdpClient.BeginReceive(System.AsyncCallback,System.Object).
        //
        //   remoteEP:
        //     The specified remote endpoint.
        //
        // Returns:
        //     If successful, the number of bytes received. If unsuccessful, this method returns
        //     0.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.UdpClient.BeginReceive(System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.UdpClient.EndReceive(System.IAsyncResult,System.Net.IPEndPoint@)
        //     was previously called for the asynchronous read.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the underlying System.Net.Sockets.Socket.
        //     See the Remarks section for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The underlying System.Net.Sockets.Socket has been closed.
        public static byte[] EndReceive(this UdpClient client, IAsyncResult asyncResult, ref IPEndPoint remoteEP)
        {
            UdpReceiveResult result = TaskToApm.End<UdpReceiveResult>(asyncResult);
            remoteEP = result.RemoteEndPoint;
            return result.Buffer;
        }

        //
        // Summary:
        //     Ends a pending asynchronous send.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult object returned by a call to Overload:System.Net.Sockets.UdpClient.BeginSend.
        //
        // Returns:
        //     If successful, the number of bytes sent to the System.Net.Sockets.UdpClient.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not returned by a call to the System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     System.Net.Sockets.Socket.EndSend(System.IAsyncResult) was previously called
        //     for the asynchronous read.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the underlying socket. See the Remarks
        //     section for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The underlying System.Net.Sockets.Socket has been closed.
        public static int EndSend(this UdpClient client, IAsyncResult asyncResult)
        {
            return TaskToApm.End<int>(asyncResult);
        }
    }

    /// <summary>
    /// Provides support for efficiently using Tasks to implement the APM (Begin/End) pattern.
    /// </summary>
    internal static class TaskToApm
    {
        /// <summary>
        /// Marshals the Task as an IAsyncResult, using the supplied callback and state
        /// to implement the APM pattern.
        /// </summary>
        /// <param name="task">The Task to be marshaled.</param>
        /// <param name="callback">The callback to be invoked upon completion.</param>
        /// <param name="state">The state to be stored in the IAsyncResult.</param>
        /// <returns>An IAsyncResult to represent the task's asynchronous operation.</returns>
        public static IAsyncResult Begin(Task task, AsyncCallback callback, object state)
        {
            Debug.Assert(task != null);

            // If the task has already completed, then since the Task's CompletedSynchronously==false
            // and we want it to be true, we need to create a new IAsyncResult. (We also need the AsyncState to match.)
            IAsyncResult asyncResult;
            if (task.IsCompleted)
            {
                // Synchronous completion.
                asyncResult = new TaskWrapperAsyncResult(task, state, completedSynchronously: true);
                if (callback != null)
                {
                    callback(asyncResult);
                }
            }
            else
            {
                // For asynchronous completion we need to schedule a callback.  Whether we can use the Task as the IAsyncResult
                // depends on whether the Task's AsyncState has reference equality with the requested state.
                asyncResult = task.AsyncState == state ? (IAsyncResult)task : new TaskWrapperAsyncResult(task, state, completedSynchronously: false);
                if (callback != null)
                {
                    InvokeCallbackWhenTaskCompletes(task, callback, asyncResult);
                }
            }
            return asyncResult;
        }

        /// <summary>Processes an IAsyncResult returned by Begin.</summary>
        /// <param name="asyncResult">The IAsyncResult to unwrap.</param>
        public static void End(IAsyncResult asyncResult)
        {
            Task task;

            // If the IAsyncResult is our task-wrapping IAsyncResult, extract the Task.
            var twar = asyncResult as TaskWrapperAsyncResult;
            if (twar != null)
            {
                task = twar.Task;
                Debug.Assert(task != null, "TaskWrapperAsyncResult should never wrap a null Task.");
            }
            else
            {
                // Otherwise, the IAsyncResult should be a Task.
                task = asyncResult as Task;
            }

            // Make sure we actually got a task, then complete the operation by waiting on it.
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.GetAwaiter().GetResult();
        }

        /// <summary>Processes an IAsyncResult returned by Begin.</summary>
        /// <param name="asyncResult">The IAsyncResult to unwrap.</param>
        public static TResult End<TResult>(IAsyncResult asyncResult)
        {
            Task<TResult> task;

            // If the IAsyncResult is our task-wrapping IAsyncResult, extract the Task.
            var twar = asyncResult as TaskWrapperAsyncResult;
            if (twar != null)
            {
                task = twar.Task as Task<TResult>;
                Debug.Assert(twar.Task != null, "TaskWrapperAsyncResult should never wrap a null Task.");
            }
            else
            {
                // Otherwise, the IAsyncResult should be a Task<TResult>.
                task = asyncResult as Task<TResult>;
            }

            // Make sure we actually got a task, then complete the operation by waiting on it.
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.GetAwaiter().GetResult();
        }

        /// <summary>Invokes the callback asynchronously when the task has completed.</summary>
        /// <param name="antecedent">The Task to await.</param>
        /// <param name="callback">The callback to invoke when the Task completes.</param>
        /// <param name="asyncResult">The Task used as the IAsyncResult.</param>
        private static void InvokeCallbackWhenTaskCompletes(Task antecedent, AsyncCallback callback, IAsyncResult asyncResult)
        {
            Debug.Assert(antecedent != null);
            Debug.Assert(callback != null);
            Debug.Assert(asyncResult != null);

            // We use OnCompleted rather than ContinueWith in order to avoid running synchronously
            // if the task has already completed by the time we get here.  This is separated out into
            // its own method currently so that we only pay for the closure if necessary.
            antecedent.ConfigureAwait(continueOnCapturedContext: false)
                      .GetAwaiter()
                      .OnCompleted(() => callback(asyncResult));

            // PERFORMANCE NOTE:
            // Assuming we're in the default ExecutionContext, the "slow path" of an incomplete
            // task will result in four allocations: the new IAsyncResult,  the delegate+closure
            // in this method, and the continuation object inside of OnCompleted (necessary
            // to capture both the Action delegate and the ExecutionContext in a single object).  
            // In the future, if performance requirements drove a need, those four 
            // allocations could be reduced to one.  This would be achieved by having TaskWrapperAsyncResult
            // also implement ITaskCompletionAction (and optionally IThreadPoolWorkItem).  It would need
            // additional fields to store the AsyncCallback and an ExecutionContext.  Once configured, 
            // it would be set into the Task as a continuation.  Its Invoke method would then be run when 
            // the antecedent completed, and, doing all of the necessary work to flow ExecutionContext, 
            // it would invoke the AsyncCallback.  It could also have a field on it for the antecedent, 
            // so that the End method would have access to the completed antecedent. For related examples, 
            // see other implementations of ITaskCompletionAction, and in particular ReadWriteTask 
            // used in Stream.Begin/EndXx's implementation.
        }

        /// <summary>
        /// Provides a simple IAsyncResult that wraps a Task.  This, in effect, allows
        /// for overriding what's seen for the CompletedSynchronously and AsyncState values.
        /// </summary>
        private sealed class TaskWrapperAsyncResult : IAsyncResult
        {
            /// <summary>The wrapped Task.</summary>
            internal readonly Task Task;
            /// <summary>The new AsyncState value.</summary>
            private readonly object _state;
            /// <summary>The new CompletedSynchronously value.</summary>
            private readonly bool _completedSynchronously;

            /// <summary>Initializes the IAsyncResult with the Task to wrap and the overriding AsyncState and CompletedSynchronously values.</summary>
            /// <param name="task">The Task to wrap.</param>
            /// <param name="state">The new AsyncState value</param>
            /// <param name="completedSynchronously">The new CompletedSynchronously value.</param>
            internal TaskWrapperAsyncResult(Task task, object state, bool completedSynchronously)
            {
                Debug.Assert(task != null);
                Debug.Assert(!completedSynchronously || task.IsCompleted, "If completedSynchronously is true, the task must be completed.");

                this.Task = task;
                _state = state;
                _completedSynchronously = completedSynchronously;
            }

            // The IAsyncResult implementation.  
            // - IsCompleted and AsyncWaitHandle just pass through to the Task.
            // - AsyncState and CompletedSynchronously return the corresponding values stored in this object.

            object IAsyncResult.AsyncState { get { return _state; } }
            bool IAsyncResult.CompletedSynchronously { get { return _completedSynchronously; } }
            bool IAsyncResult.IsCompleted { get { return this.Task.IsCompleted; } }
            WaitHandle IAsyncResult.AsyncWaitHandle { get { return ((IAsyncResult)this.Task).AsyncWaitHandle; } }
        }
    }
}
