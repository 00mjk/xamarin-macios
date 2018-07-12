//
// NWEndpoint.cs: Bindings the Netowrk nw_endpoint_t API.
//
// Authors:
//   Miguel de Icaza (miguel@microsoft.com)
//
// Copyrigh 2018 Microsoft Inc
//
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using ObjCRuntime;
using Foundation;
using CoreFoundation;

using OS_nw_protocol_definition=System.IntPtr;

namespace Network {

	public enum NWIPVersion {
		Any = 0,
		Version4 = 1,
		Version6 = 2
	}
		
	public class NWProtocolDefinition : NativeObject {
		public NWProtocolDefinition (IntPtr handle, bool owns) : base (handle, owns) {}

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		[return: MarshalAs (UnmanagedType.I1)]			
		static extern bool nw_protocol_definition_is_equal (OS_nw_protocol_definition definition1, OS_nw_protocol_definition definition2);

		public bool Equals (object other)
		{
			if (other == null)
				return false;
			if (!(other is NWProtocolDefinition))
				return false;
			return nw_protocol_definition_is_equal (GetHandle(), (other as NWProtocolDefinition).handle);
		}

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		static extern OS_nw_protocol_definition nw_protocol_copy_ip_definition ();
		
		[TV (12,0), Mac (10,14), iOS (12,0)]
		public static NWProtocolDefinition IPDefinition => new NWProtocolDefinition (nw_protocol_copy_ip_definition (), owns: true);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		static extern OS_nw_protocol_definition nw_protocol_copy_tcp_definition ();

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public static NWProtocolDefinition TcpDefinition => new NWProtocolDefinition (nw_protocol_copy_tcp_definition (), owns: true);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		static extern OS_nw_protocol_definition nw_protocol_copy_udp_definition ();

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public static NWProtocolDefinition UdpDefinition => new NWProtocolDefinition (nw_protocol_copy_udp_definition (), owns: true);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		static extern OS_nw_protocol_definition nw_protocol_copy_tls_definition ();

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public static NWProtocolDefinition TlsDefinition => new NWProtocolDefinition (nw_protocol_copy_tls_definition (), owns: true);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		static extern OS_nw_protocol_definition nw_tcp_create_options ();

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public NWProtocolDefinition CreateTcpOptions ()
		{
			return new NWProtocolDefinition (nw_tcp_create_options (), owns: true);
		}

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		static extern OS_nw_protocol_definition nw_udp_create_options ();

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public NWProtocolDefinition CreateUdpOptions ()
		{
			return new NWProtocolDefinition (nw_udp_create_options (), owns: true);
		}

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		static extern OS_nw_protocol_definition nw_tls_create_options ();

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public NWProtocolDefinition CreateTlsOptions ()
		{
			return new NWProtocolDefinition (nw_tls_create_options (), owns: true);
		}

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_no_delay (IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool noDelay);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetNoDelay (bool noDelay) => nw_tcp_options_set_no_delay (GetHandle(), noDelay);
		
		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_no_push(IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool noPush);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetNoPush (bool noPush) => nw_tcp_options_set_no_push (GetHandle(), noPush);
			
		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_no_options(IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool noOptions);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetNoOptions (bool noOptions) => nw_tcp_options_set_no_options (GetHandle(), noOptions);
		

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_enable_keepalive(IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool enableKeepalive);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetEnableKeepAlive (bool enableKeepalive) => nw_tcp_options_set_enable_keepalive (GetHandle(), enableKeepalive);
			
		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_keepalive_count(IntPtr handle, uint keepaliveCount);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetKeepaliveCount (uint keepaliveCount) => nw_tcp_options_set_keepalive_count (GetHandle(), keepaliveCount);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_keepalive_idle_time(IntPtr handle, uint keepaliveIdleTime);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetKeepaliveIdleTime (uint keepaliveIdleTime) => nw_tcp_options_set_keepalive_idle_time (GetHandle(), keepaliveIdleTime);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_keepalive_interval(IntPtr handle, uint keepaliveInterval);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetKeepaliveInterval (uint keepaliveInterval) => nw_tcp_options_set_keepalive_interval (GetHandle(), keepaliveInterval);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_maximum_segment_size(IntPtr handle, uint maximumSegmentSize);
		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetMaximumSegmentSize (uint maximumSegmentSize) => nw_tcp_options_set_maximum_segment_size (GetHandle(), maximumSegmentSize);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_connection_timeout(IntPtr handle, uint connectionTimeout);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetConnectionTimeout (uint connectionTimeout) => nw_tcp_options_set_connection_timeout (GetHandle(), connectionTimeout);
		
		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_persist_timeout(IntPtr handle, uint persistTimeout);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetPersistTimeout (uint persistTimeout) => nw_tcp_options_set_persist_timeout (GetHandle(), persistTimeout);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_retransmit_connection_drop_time (IntPtr handle, uint retransmitConnectionDropTime);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetRetransmitConnectionDropTime (uint retransmitConnectionDropTime) => nw_tcp_options_set_retransmit_connection_drop_time (GetHandle(), retransmitConnectionDropTime);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_retransmit_fin_drop (IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool retransmitFinDrop);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetRetransmitFinDrop (bool retransmitFinDrop) => nw_tcp_options_set_retransmit_fin_drop (GetHandle(), retransmitFinDrop);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_disable_ack_stretching (IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool disableAckStretching);
		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetDisableAckStretching (bool disableAckStretching) => nw_tcp_options_set_disable_ack_stretching (GetHandle(), disableAckStretching);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_enable_fast_open (IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool enableFastOpen);
		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetEnableFastOpen (bool enableFastOpen) => nw_tcp_options_set_enable_fast_open (GetHandle(), enableFastOpen);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_tcp_options_set_disable_ecn (IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool disableEcn);
		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void TcpSetDisableEcn (bool disableEcn) => nw_tcp_options_set_disable_ecn (GetHandle(), disableEcn);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		[DllImport (Constants.NetworkLibrary)]
		extern static void nw_udp_options_set_prefer_no_checksum (IntPtr handle, [MarshalAs (UnmanagedType.U1)] bool preferNoChecksums);

		[TV (12,0), Mac (10,14), iOS (12,0)]
		public void UdpSetPreferNoChecksum (bool preferNoChecksums) => nw_udp_options_set_prefer_no_checksum (GetHandle(), preferNoChecksums);
	}
}
