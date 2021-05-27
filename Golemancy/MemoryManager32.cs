using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Golemancy {
	class MemoryManager32 {
		[DllImport("kernel32.dll")]
	    public static extern Boolean ReadProcessMemory (
			IntPtr hProcess,
			IntPtr lpBaseAddress,
			Byte[] lpBuffer,
			Int32 dwSize,
			out Int32 lpNumberOfBytesRead );

	    [DllImport("kernel32.dll")]
	    public static extern Boolean WriteProcessMemory (
			IntPtr hProcess,
			IntPtr lpBaseAddress,
			Byte[] lpBuffer,
			Int32 dwSize,
			out Int32 lpNumberOfBytesWritten );

		[DllImport("kernel32.dll")]
		public static extern Int32 VirtualQueryEx (
			IntPtr hProcess,
			IntPtr lpAddress,
			out MEMORY_BASIC_INFORMATION32 lpBuffer,
			UInt32 dwLength );
		
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

			[DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [Out] IntPtr lphModule,
            UInt32 cb,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpcbNeeded,
            UInt32 dwff);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(
            IntPtr hProcess, 
            IntPtr hModule, 
            [Out] StringBuilder lpBaseName, 
            [In] [MarshalAs(UnmanagedType.U4)] int nSize);

		public IntPtr GetModuleBaseAddress ( Process process, String name ) {
			foreach ( ProcessModule module in process.Modules ) {
				if ( module.ModuleName.Contains(name) )
					return module.BaseAddress;
			}

			return IntPtr.Zero;
		}

		public IntPtr GetModuleHandle ( Process process, String name ) {
			IntPtr handle = IntPtr.Zero;

			IntPtr[] handles = new IntPtr[1024];

			GCHandle gch = GCHandle.Alloc(handles, GCHandleType.Pinned);
			IntPtr pModules = gch.AddrOfPinnedObject();

			UInt32 size = (UInt32)(Marshal.SizeOf(typeof(IntPtr)) * handles.Length);
			if ( EnumProcessModulesEx(process.Handle, pModules, size, out UInt32 bytecount, 0x03) ) {
				Int32 count = (Int32) (bytecount / Marshal.SizeOf(typeof(IntPtr)));

				for ( Int32 i = 0 ; i < count ; ++i ) {
					StringBuilder strbld = new StringBuilder(1024);

					GetModuleFileNameEx(process.Handle, handles[i], strbld, strbld.Capacity);
					if ( strbld.ToString().Contains(name) )
						handle = handles[i];
				}
			}

			gch.Free();

			return handle;
		}

		public static List<Int32> FindBytePattern ( Process process, Int32 start, Int32 stop, Byte?[] pattern ) {
			List<Int32> matches = new List<Int32>();

			Int32 currentAddress = start;
			while ( currentAddress < stop ) {
				if ( VirtualQueryEx(process.Handle, (IntPtr) currentAddress, out MEMORY_BASIC_INFORMATION32 memoryRegion, (UInt32) Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION32))) > 0
					&& memoryRegion.RegionSize > 0
					&& memoryRegion.State == 0x1000 ) { // MEM_COMMIT

					Int32 regionStartAddress = memoryRegion.BaseAddress;
					if ( start > regionStartAddress )
						regionStartAddress = start;

					Int32 regionEndAddress = memoryRegion.BaseAddress + memoryRegion.RegionSize;
					if ( stop < regionEndAddress )
						regionEndAddress = stop;

					Byte[] regionBytes = new Byte[regionEndAddress - regionStartAddress];

					Console.WriteLine($"Scanning {regionStartAddress:X8}-{regionEndAddress:X8}");
					ReadProcessMemory(process.Handle, (IntPtr) regionStartAddress, regionBytes, regionBytes.Length, out Int32 lpNumberOfBytesRead);

					if ( regionBytes.Length == 0 || pattern.Length == 0 || regionBytes.Length < pattern.Length )
						continue;

					List<Int32> matchedIndices = new List<Int32>();
					Int32[] longestPrefixSuffices = new Int32[pattern.Length];

					GetLongestPrefixSuffices(pattern, ref longestPrefixSuffices);

					Int32 textIndex = 0;
					Int32 patternIndex = 0;

					while ( textIndex < regionBytes.Length ) {
						if ( !pattern[patternIndex].HasValue
							|| regionBytes[textIndex] == pattern[patternIndex] ) {
							++textIndex;
							++patternIndex;
						}

						if ( patternIndex == pattern.Length ) {
							matchedIndices.Add(textIndex - patternIndex);
							patternIndex = longestPrefixSuffices[patternIndex - 1];
						} else if ( textIndex < regionBytes.Length
								&& (pattern[patternIndex].HasValue
								&& regionBytes[textIndex] != pattern[patternIndex]) ) {
							if ( patternIndex != 0 )
								patternIndex = longestPrefixSuffices[patternIndex - 1];
							else
								++textIndex;
						}
					}

					foreach ( var matchIndex in matchedIndices ) {
						Console.WriteLine($"Found match at {regionStartAddress + matchIndex:X8}");
						matches.Add(regionStartAddress + matchIndex);
					}
				}
				currentAddress = memoryRegion.BaseAddress + memoryRegion.RegionSize;
			}

			return matches;
		}

		static void GetLongestPrefixSuffices( Byte?[] pattern, ref Int32[] longestPrefixSuffices ) {
			Int32 patternLength = pattern.Length;
			Int32 length = 0;
			Int32 patternIndex = 1;

			longestPrefixSuffices[0] = 0;

			while ( patternIndex < patternLength ) {
				if ( pattern[patternIndex] == pattern[length] ) {
					++length;
					longestPrefixSuffices[patternIndex] = length;
					++patternIndex;
				} else {
					if ( length == 0 ) {
						longestPrefixSuffices[patternIndex] = 0;
						++patternIndex;
					} else
						length = longestPrefixSuffices[length - 1];
				}
			}
		}

		public static T Read<T> ( Process process, Int32 address ) where T : struct {
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
		public static String ReadNTString ( Process process, Int32 address ) {
			Byte[] read = new Byte[200];

			ReadProcessMemory((IntPtr) process.Handle, (IntPtr) address, read, 200, out Int32 lpNumberOfBytesRead);

			Int32 nullindex = 0;
			while ( nullindex < lpNumberOfBytesRead && read[nullindex] != 0)
				nullindex += 1;

			return Encoding.UTF8.GetString(read, 0, nullindex);
		}
		public static String ReadNTStringAt ( Process process, Int32 address ) {
			Int32 stringAddress = Read<Int32>(process, address);
			return ReadNTString(process, stringAddress);
		}
		public static String ReadString ( Process process, Int32 address ) {
			Int32 stringAddress = Read<Int32>(process, address);
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
