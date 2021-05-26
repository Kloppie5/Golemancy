using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Golemancy {
	class CultistSimulatorController : MemoryManager32 {
		Process _process;
		public CultistSimulatorController ( Process process ) {
			_process = process;
		}
		// ?? ?? ?? ?? F9 FF FF 1F 70 2E ?? 08 ?? ?1 ?? 1D F? ?? ?? 13 78 09 00 00 00 01 00 00 04 00 00 00 00 00 00 00 00 00 00 00 18 00 ?? 0? 24 00 ?? 0? 30 00 ?? 0? ?? B? ?? 19 48 00 ?? 0? 54 00 ?? 0? B0 7D ?? 08 00 00 00 00 00 00 96 43 ?? ?? ?? ?? ?? ?? ?? 13 ?? ?? ?? 18 ?? ?? ?? 1A 0B 00 00 00 43 00 00 00 00 00 04 40 00 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ??

		// 58 EA 1F 13 F9 FF FF 1F 70 2E C9 08 E0 51 FE 1D F0 98 6B 13 78 09 00 00 00 01 00 00 04 00 00 00 00 00 00 00 00 00 00 00 18 00 20 09 24 00 20 09 30 00 20 09 F0 BC 71 19 48 00 20 09 54 00 20 09 B0 7D CB 08 00 00 00 00 00 00 96 43 00 00 00 00 28 E1 6B 13 88 59 6C 18 78 A7 6C 1A 0B 00 00 00 43 00 00 00 00 00 04 40 00 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 C8 98 6B 13 00 00 00 00 C0 A7 6C 1A 0A 00 00 00 48 00 00 00 00 00 00 40 00 00 00 00 00 00 00 00 40 DA 58 13 01 00 00 18 70 2E C9 08 00 A0 FD 1D C8 E0 63 13 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 18 00 20 09 24 00 20 09 30 00 20 09 3C 00 20 09 00 00 00 00 00 00 00 00 00 AB 7A 9D 00 13 00 88 F8 6E 2A 19 00 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 06 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
		// 18 C7 FE 12 F9 FF FF 1F 70 2E A5 08 C0 71 DD 1D F8 83 4A 13 78 09 00 00 00 01 00 00 04 00 00 00 00 00 00 00 00 00 00 00 18 00 FC 08 24 00 FC 08 30 00 FC 08 B8 BD 64 19 48 00 FC 08 54 00 FC 08 B0 7D A7 08 00 00 00 00 00 00 96 43 8C 47 86 02 30 CC 4A 13 10 DC 3F 18 D8 9D 33 1A 0B 00 00 00 43 00 00 00 00 00 04 40 00 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 43 F6 18 D0 83 4A 13 00 00 00 00 20 9E 33 1A 0A 00 00 00 48 00 00 00 00 00 00 40 00 00 00 00 00 00 00 00 C0 09 38 13 01 00 00 18 70 2E A5 08 E0 7F DD 1D 60 51 42 13 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 18 00 FC 08 24 00 FC 08 30 00 FC 08 3C 00 FC 08 E0 51 42 13 D8 76 81 18 68 9E 33 1A 0A 00 00 00 08 00 00 00 00 00 04 40 00 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 48 3C 44 13 01 00 00 18 70 2E A5 08 A0 B8 DC 1D 18 3E 44 13 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 18 00 FC 08 24 00 FC 08 30 00 FC 08 3C 00 FC 08 00 00 00 40 00 00 00 00 00 00 80 40 00 00 00 00 CD CC CC 3D 00 00 00 00 9A 99 19 3F 00 00 00 00 9A 99 19 3F 00 00 00 00 D0 3E 44 13 00 00 00 00 70 9E 33 1A 0B 00 00 00 44 00 00 00 00 00 04 40 00 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 38 01 FC 08 44 01 FC 08 50 01 FC 08 5C 01 FC 08 68 01 FC 08 74 01 FC 08 80 01 FC 08 8C 01 FC 08 98 01 FC 08 A4 01 FC 08 B0 01 FC 08 BC 01 FC 08 C8 01 FC 08

		// name: MonoVTable + 0x2C
		// name_space: MonoVTable + 0x30
		// TabletopManager: 54 61 62 6C 65 74 6F 70 4D 61 6E 61 67 65 72 00
		// Assets.CS.TabletopUI: 41 73 73 65 74 73 2E 43 53 2E 54 61 62 6C 65 74 6F 70 55 49 00

		public Int32 FindTabletopManager ( ) {
			Int32 monoBase = (Int32) GetModuleBaseAddress(_process, "mono-2.0-bdwgc.dll");

			Int32 root_domain = Read<Int32>(monoBase + 0x3A41AC);
			String friendly_name = ReadNullTerminatedString(root_domain + 0x74);
			Console.WriteLine($"Domain \"{friendly_name}\" at {root_domain:X8}");

			Int32 ASSEMBLIES_DLL = Read<Int32>(root_domain + 0x6C);
			Int32 DATA;
			Int32 NEXT = -1;
			Int32 PREV;
			for ( Int32 CURR = ASSEMBLIES_DLL ; NEXT != 0 ; CURR = NEXT ) {
				DATA = Read<Int32>(CURR);
				NEXT = Read<Int32>(CURR + 0x4);
				PREV = Read<Int32>(CURR + 0x8);
				Int32 refcount = Read<Int32>(DATA);
				String baseDir = ReadNullTerminatedString(DATA + 0x4);
				String name = ReadNullTerminatedString(DATA + 0x8);
				Int32 MonoImage = Read<Int32>(DATA + 0x44);
				// Console.WriteLine($"Found Assembly \"{name}\" at {DATA:X8} <{PREV:X8}|{NEXT:X8}>");

				String fileName = ReadNullTerminatedString(MonoImage + 0x14);
				String assemblyName = ReadNullTerminatedString(MonoImage + 0x18);
				String moduleName = ReadNullTerminatedString(MonoImage + 0x1C);
				if ( moduleName == "Assembly-CSharp.dll" )
					Console.WriteLine($"Found Image {moduleName} at {MonoImage:X8}");
			}

			Int32 nameAddress = FindBytePattern(_process, 0, 0x21000000, new Byte?[] { 0x41, 0x73, 0x73, 0x65, 0x74, 0x73, 0x2E, 0x43, 0x53, 0x2E, 0x54, 0x61, 0x62, 0x6C, 0x65, 0x74, 0x6F, 0x70, 0x55, 0x49, 0x00 }).First();
			Console.WriteLine($"Found name at {nameAddress:X8}");
			Int32 namespaceAddress;



			return 0;
		}

		public Int32 FindSituationsCatalogue ( ) {
			return 0;
		}

		public T Read<T> ( Int32 address ) where T : struct {
			return Read<T>(_process, address);
		}
		public String ReadNullTerminatedString ( Int32 address ) {
			return ReadNullTerminatedString(_process, address);
		}
		public String ReadString ( Int32 address ) {
			return ReadString(_process, address);
		}
	}
}
