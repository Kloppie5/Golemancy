using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Golemancy {
	class CultistSimulatorMemoryManager {
		[DllImport("kernel32.dll")]
	    public static extern Boolean ReadProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, Byte[] lpBuffer, Int32 dwSize, out Int32 lpNumberOfBytesRead );
	    [DllImport("kernel32.dll")]
	    public static extern Boolean WriteProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, Byte[] lpBuffer, Int32 dwSize, out Int32 lpNumberOfBytesWritten );

		/**
			Generic rant 1; Who in their right mind thought it would be a good idea to hide the complex yet beautiful C++ types with the mess that is IntPtr and its ramifications for 64-bit code that interacts with 32-bit code?
			Generic rant 2; I know that array indexing is basically just syntactic sugar for pointer arithmetic, but I still think it's a terrible design choice to make array indexing require signed types.
		*/

		public UInt32 FindTabletopManager ( Process process ) {
			UInt32 start = 0x08CF0000;
			UInt32 stop  = 0x09000000;

			Byte[] region = new Byte[stop-start];
			ReadProcessMemory(process.Handle, (IntPtr) start, region, (Int32) (stop-start), out Int32 lpNumberOfBytesRead);

			for ( Int32 c = 0 ; c < stop-start-8 ; c += 4 ) {

				if (
					BitConverter.ToUInt32(region, c + 0) == 0x19355C58 // vtable
				&&  BitConverter.ToUInt32(region, c + 4) == 0x00000000 // .NET locking
				&&  BitConverter.ToUInt32(region, c + 8) != 0x00000000 // UnityEngine.Object.m_CachedPtr
				) {
					return start + (UInt32) c;
				}
			}

			return 0;
		}

		public static T Read<T> ( Process process, UInt32 address ) where T : struct {
			Int32 size = Marshal.SizeOf(typeof(T));
			Byte[] read = new Byte[size];

			ReadProcessMemory(process.Handle, (IntPtr) address, read, size, out Int32 lpNumberOfBytesRead);

			T result;
			GCHandle handle = GCHandle.Alloc(read, GCHandleType.Pinned);

			try {
				result = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			} finally {
				handle.Free();
			}

			return result;
		}
		public static String ReadString ( Process process, UInt32 address ) {
			UInt32 stringAddress = Read<UInt32>(process, address);
			Int32 size = Read<Int32>(process, stringAddress + 0x8);
			if ( size < 0 || size > 1000 )
				return "INVALID STRING";
			Byte[] read = new Byte[size*2];

			ReadProcessMemory((IntPtr) process.Handle, (IntPtr) stringAddress + 0xC, read, size*2, out Int32 lpNumberOfBytesRead);

			Int32 nullindex = 0;
			while ( nullindex < lpNumberOfBytesRead && read[nullindex] != 0)
				nullindex += 2;

			return Encoding.Unicode.GetString(read, 0, nullindex);
		}
	}
}
