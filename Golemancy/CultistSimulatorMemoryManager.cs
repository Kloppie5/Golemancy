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
		[DllImport("kernel32.dll")]
		public static extern Int32 VirtualQueryEx( IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION32 lpBuffer, UInt32 dwLength );
		
		public struct MEMORY_BASIC_INFORMATION32 {
			public Int32 BaseAddress;
			public Int32 AllocationBase;
			public Int32 AllocationProtect;
			public Int32 RegionSize;
			public Int32 State;
			public Int32 Protect;
			public Int32 Type;
		}

		/**
			Generic rant 1; Who in their right mind thought it would be a good idea to hide the complex yet beautiful C++ types with the mess that is IntPtr and its ramifications for 64-bit code that interacts with 32-bit code?
			Generic rant 2; I know that array indexing is basically just syntactic sugar for pointer arithmetic, but I still think it's a terrible design choice to make array indexing require signed types.
		*/

		public UInt32 FindTabletopManager ( Process process ) {
			return FindPatternAddress(process, 0x0, 0x21000000, ( bytes, index ) => {
				return
				   index < bytes.Length - 8
				&& BitConverter.ToUInt32(bytes, index + 0) == 0x19428DB8 // vtable
				&& BitConverter.ToUInt32(bytes, index + 4) == 0x00000000 // .NET locking
				&& BitConverter.ToUInt32(bytes, index + 8) != 0x00000000; // UnityEngine.Object.m_CachedPtr
			});
		}

		public UInt32 FindSituationsCatalogue ( Process process ) {
			return FindPatternAddress(process, 0x0, 0x21000000, ( bytes, index ) => {
				return
				   index < bytes.Length - 8
				&& BitConverter.ToUInt32(bytes, index + 0) == 0x192BF210 // vtable
				&& BitConverter.ToUInt32(bytes, index + 4) == 0x00000000 // .NET locking
				&& BitConverter.ToUInt32(bytes, index + 8) != 0x00000000; // _currentSituationControllers
			});
		}

		public UInt32 FindPatternAddress ( Process process, Int32 start, Int32 stop, Func<Byte[], Int32, Boolean> pattern ) {
			Console.WriteLine($"Scanning {start:X8}-{stop:X8}");
			Int32 currentAddress = start;
			while ( currentAddress < stop ) {
				if ( VirtualQueryEx(process.Handle, (IntPtr) currentAddress, out MEMORY_BASIC_INFORMATION32 memoryRegion, (UInt32) Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION32))) > 0
					&& memoryRegion.RegionSize > 0
					&& memoryRegion.State == 0x1000 // MEM_COMMIT
					) {

					Int32 regionStartAddress = memoryRegion.BaseAddress;
					if ( start > regionStartAddress )
						regionStartAddress = start;

					Int32 regionEndAddress = memoryRegion.BaseAddress + memoryRegion.RegionSize;
					if ( stop < regionEndAddress )
						regionEndAddress = stop;

					Byte[] regionBytes = new Byte[regionEndAddress - regionStartAddress];
					ReadProcessMemory(process.Handle, (IntPtr) regionStartAddress, regionBytes, regionBytes.Length, out Int32 lpNumberOfBytesRead);

					for ( Int32 testIndex = 0 ; testIndex < regionBytes.Length ; testIndex += 4 ) {
						if ( pattern.Invoke(regionBytes, testIndex) )
							return (UInt32) (regionStartAddress + testIndex);
					}
				}
				currentAddress = memoryRegion.BaseAddress + memoryRegion.RegionSize;
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
