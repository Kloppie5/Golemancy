using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Golemancy {
	class CultistSimulatorController : MonoManager32 {
		public CultistSimulatorController ( Process process ) : base(process) {

		}

		public Int32 FindTabletopManager ( ) {
			Int32 monoBase = (Int32) GetModuleBaseAddress("mono-2.0-bdwgc.dll");

			Int32 root_domain = Read<Int32>(monoBase + 0x3A41AC);
			String friendly_name = ReadUTF8StringAt(root_domain + 0x74);
			Console.WriteLine($"Domain \"{friendly_name}\" at {root_domain:X8}");

			Int32 TabletopManagerMonoClassAddress = -1;

			Int32 ASSEMBLIES_DLL = Read<Int32>(root_domain + 0x6C);
			Int32 DATA;
			Int32 NEXT = -1;
			Int32 PREV;
			for ( Int32 CURR = ASSEMBLIES_DLL ; NEXT != 0 ; CURR = NEXT ) {
				DATA = Read<Int32>(CURR);
				NEXT = Read<Int32>(CURR + 0x4);
				PREV = Read<Int32>(CURR + 0x8);
				Int32 refcount = Read<Int32>(DATA);
				String baseDir = ReadUTF8StringAt(DATA + 0x4);
				String name = ReadUTF8StringAt(DATA + 0x8);
				Int32 MonoImage = Read<Int32>(DATA + 0x44);
				if ( name != "Assembly-CSharp" )
					continue;
				Console.WriteLine($"Found Assembly \"{name}\" at {DATA:X8} <{PREV:X8}|{NEXT:X8}>");
				Console.WriteLine($"With MonoImage at {MonoImage:X8}");
				String fileName = ReadUTF8StringAt(MonoImage + 0x14);
				String assemblyName = ReadUTF8StringAt(MonoImage + 0x18);
				String moduleName = ReadUTF8StringAt(MonoImage + 0x1C);
				String version = ReadUTF8StringAt(MonoImage + 0x20);
				String guid = ReadUTF8StringAt(MonoImage + 0x28);
				Int32 MonoCLIImageInfo = Read<Int32>(MonoImage + 0x2C);
				Int32 MonoMemPool = Read<Int32>(MonoImage + 0x30);
				Int32 HeapString = Read<Int32>(MonoImage + 0x38);
				Console.WriteLine($"Module HeapString at {HeapString:X8}");
				Int32 HeapStringSize = Read<Int32>(MonoImage + 0x3C);
				Int32 HeapUs = Read<Int32>(MonoImage + 0x40);
				Int32 HeapUsSize = Read<Int32>(MonoImage + 0x44);
				Int32 HeapBlob = Read<Int32>(MonoImage + 0x48);
				Int32 HeapBlobSize = Read<Int32>(MonoImage + 0x4C);
				Int32 HeapGuid = Read<Int32>(MonoImage + 0x50);
				Int32 HeapGuidSize = Read<Int32>(MonoImage + 0x54);
				Int32 HeapTables = Read<Int32>(MonoImage + 0x58);
				Int32 HeapTablesSize = Read<Int32>(MonoImage + 0x5C);

				Int32 TABLES_base = Read<Int32>(MonoImage + 0x68);

				Int32 MonoTableModule = Read<Int32>(MonoImage + 0x7C);

				Int32 TYPE_DEF_base = Read<Int32>(MonoImage + 0x94);
				Int32 dimRows = Read<Int32>(MonoImage + 0x98);
				Int32 rowCount = dimRows & 0xFFFFFF;
				Int32 rowSize = dimRows >> 24;
				Int32 dimCols = Read<Int32>(MonoImage + 0x9C);
				Int32 colCount = dimCols >> 24;
				// Cols {FLAGS, NAME, NAMESPACE, EXTENDS, FIELD_LIST, METHOD_LIST}

				Console.WriteLine($"Analysing {rowCount}x{colCount} table at {TYPE_DEF_base:X8};");

				Int32 TabletopManagerRow = -1;
				for ( Int32 i = 0 ; i < rowCount ; ++i ) {
					Int32 pointer = TYPE_DEF_base + i * rowSize;
					Int32[] row = new Int32[colCount];
					for ( Int32 j = 0 ; j < colCount ; ++j ) {
						Int32 n = (((dimCols) >> (j * 2)) & 0x3) + 1;

						switch (n) {
							case 1: row[j] = Read<Byte>(pointer); break;
							case 2: row[j] = Read<Int16>(pointer); break;
							case 4: row[j] = Read<Int32>(pointer); break;
						}
						pointer += n;
					}
					String className = ReadUTF8String(HeapString + row[1]);
					if ( className == "TabletopManager" )
						TabletopManagerRow = i;
					Console.WriteLine($"Class {i}; name \"{className}\" at {HeapString + row[1]:X8}");

					// RuntimeInformation-> domain_vtables [root_domain->domain_id]
				}
				
				Int32 assembly = Read<Int32>(MonoImage + 0x34C);
				Console.WriteLine($"Assembly link {assembly:X8}");
				Int32 class_cache_hash_func = Read<Int32>(MonoImage + 0x354); // hash function doesnt change anything
				Int32 class_cache_key_extract = Read<Int32>(MonoImage + 0x358); // reads +0x34
				Int32 class_cache_next_value = Read<Int32>(MonoImage + 0x35C); // adds 0xA8
				Int32 class_cache_size = Read<Int32>(MonoImage + 0x360);
				Int32 class_cache_num_entries = Read<Int32>(MonoImage + 0x364);
				Int32 class_cache_table = Read<Int32>(MonoImage + 0x368);
				Console.WriteLine($"class_cache at {class_cache_table:X8}");

				Int32 TabletopManagerIndex = TabletopManagerRow + 1;
				Int32 TabletopManagerTypeToken = 0x02000000 | TabletopManagerIndex;
				Int32 MonoClassPointer = Read<Int32>(class_cache_table + (TabletopManagerTypeToken % class_cache_size) * 4);
				while ( MonoClassPointer != 0 ) {
					if ( Read<Int32>(Read<Int32>(MonoClassPointer) + 0x34) == TabletopManagerTypeToken )
						break;
					MonoClassPointer = Read<Int32>(MonoClassPointer + 0xA8);
					Console.WriteLine($"New pointer {MonoClassPointer:X8}");
				}
				Int32 MonoClass = Read<Int32>(MonoClassPointer);
				String classname = ReadUTF8StringAt(MonoClass + 0x2C);
				String classnamespace = ReadUTF8StringAt(MonoClass + 0x30);
				Int32 ClassType = Read<Int32>(MonoClass + 0x34);
				Console.WriteLine($"Class {classnamespace} . {classname} ({TabletopManagerTypeToken:X8} = {ClassType:X8}) at {MonoClass:X8}");
				TabletopManagerMonoClassAddress = MonoClass;
			}

			Byte?[] TabletopManagerMonoVTablePattern = new Byte?[] {
				(Byte) TabletopManagerMonoClassAddress,
				(Byte) (TabletopManagerMonoClassAddress >> 8),
				(Byte) (TabletopManagerMonoClassAddress >> 16),
				(Byte) (TabletopManagerMonoClassAddress >> 24),
				0xF9,
				0xFF,
				0xFF,
				0x1F,
				(Byte) root_domain,
				(Byte) (root_domain >> 8),
				(Byte) (root_domain >> 16),
				(Byte) (root_domain >> 24)
			};

			Console.WriteLine($"Looking for {TabletopManagerMonoVTablePattern}");

			Int32 MonoVTableAddress = FindBytePattern(0, 0x21000000, TabletopManagerMonoVTablePattern).First();
			Int32 MonoClassCheck = Read<Int32>(MonoVTableAddress);
			String ClassNameCheck = ReadUTF8StringAt(MonoClassCheck + 0x2C);
			Console.WriteLine($"Found VTable for class {ClassNameCheck} at {MonoVTableAddress:X8}");
			//Int32 namespaceAddress;



			return 0;
		}

		public Int32 FindSituationsCatalogue ( ) {
			return 0;
		}
	}
}
