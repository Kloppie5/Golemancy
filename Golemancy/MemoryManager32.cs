using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Golemancy {
	class MemoryManager32 {

		protected Process _process;

		public MemoryManager32 ( Process process ) {
			_process = process;
		}

		public Int32 GetModuleBaseAddress ( String name ) {
			foreach ( ProcessModule module in _process.Modules ) {
				if ( module.ModuleName.Contains(name) )
					return (Int32) module.BaseAddress;
			}

			return 0;
		}

		public List<Int32> FindBytePattern ( Int32 start, Int32 stop, Byte?[] pattern ) {
			List<Int32> matches = new List<Int32>();

			Int32 currentAddress = start;
			while ( currentAddress < stop ) {
				if (
					WinAPI32.VirtualQueryEx(
						_process.Handle,
						currentAddress,
						out WinAPI32.MEMORY_BASIC_INFORMATION32 memoryRegion, 
						(UInt32) Marshal.SizeOf(typeof(WinAPI32.MEMORY_BASIC_INFORMATION32))
					) > 0
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

					WinAPI32.ReadProcessMemory(_process.Handle, regionStartAddress, regionBytes, regionBytes.Length, out _);

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

		public T Read<T> ( Int32 address ) where T : struct {
			Int32 size = Marshal.SizeOf(typeof(T));
			Byte[] read = new Byte[size];

			WinAPI32.ReadProcessMemory(_process.Handle, address, read, size, out _);

			T result;
			GCHandle handle = GCHandle.Alloc(read, GCHandleType.Pinned);

			try {
				result = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			} finally {
				handle.Free();
			}

			return result;
		}
		// NOTE: used arbitrary 200 character limit, should be removed
		public String ReadUTF8String ( Int32 address ) {
			Byte[] read = new Byte[200];

			WinAPI32.ReadProcessMemory(_process.Handle, address, read, 200, out Int32 lpNumberOfBytesRead);

			Int32 nullindex = 0;
			while ( nullindex < lpNumberOfBytesRead && read[nullindex] != 0)
				nullindex += 1;

			return Encoding.UTF8.GetString(read, 0, nullindex);
		}
		public String ReadUTF8StringAt ( Int32 address ) {
			return ReadUTF8String(Read<Int32>(address));
		}
		public String ReadUnicodeString ( Int32 address, Int32 size ) {
			Byte[] read = new Byte[size*2];
			
			WinAPI32.ReadProcessMemory(_process.Handle, address, read, size*2, out Int32 lpNumberOfBytesRead);

			Int32 nullindex = 0;
			while ( nullindex < lpNumberOfBytesRead && read[nullindex] != 0)
				nullindex += 2;

			return Encoding.Unicode.GetString(read, 0, nullindex);
		}
		public String ReadUnicodeStringAt ( Int32 address ) {
			Int32 stringAddress = Read<Int32>(address);
			Int32 size = Read<Int32>(stringAddress + 0x4);

			return ReadUnicodeString(stringAddress + 0x8, size);
		}
	}
}
