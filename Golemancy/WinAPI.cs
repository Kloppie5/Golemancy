using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Golemancy {
	class WinAPI32 {
		#region kernel32
			[DllImport("kernel32.dll")]
			public static extern Boolean ReadProcessMemory (
				IntPtr hProcess,
				Int32 lpBaseAddress,
				Byte[] lpBuffer,
				Int32 dwSize,
				out Int32 lpNumberOfBytesRead );

			[DllImport("kernel32.dll")]
			public static extern Boolean WriteProcessMemory (
				IntPtr hProcess,
				Int32 lpBaseAddress,
				Byte[] lpBuffer,
				Int32 dwSize,
				out Int32 lpNumberOfBytesWritten );

			[DllImport("kernel32.dll")]
			public static extern UInt32 VirtualQueryEx (
				IntPtr hProcess,
				Int32 lpAddress,
				out MEMORY_BASIC_INFORMATION32 lpBuffer,
				UInt32 dwLength );
		#endregion
		#region winnt
			public struct MEMORY_BASIC_INFORMATION32 {
				public Int32 BaseAddress;
				public Int32 AllocationBase;
				public Int32 AllocationProtect;
				public Int32 RegionSize;
				public Int32 State;
				public Int32 Protect;
				public Int32 Type;
			}
		#endregion
	}
}
