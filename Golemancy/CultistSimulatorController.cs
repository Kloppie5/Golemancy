﻿using System;
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

		public Int32 FindTabletopManager ( ) {
			Int32 monoBase = (Int32) GetModuleBaseAddress(_process, "mono-2.0-bdwgc.dll");

			Int32 root_domain = Read<Int32>(monoBase + 0x3A41AC);
			String friendly_name = ReadNTStringAt(root_domain + 0x74);
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
				String baseDir = ReadNTStringAt(DATA + 0x4);
				String name = ReadNTStringAt(DATA + 0x8);
				Int32 MonoImage = Read<Int32>(DATA + 0x44);
				if ( name != "Assembly-CSharp" )
					continue;
				Console.WriteLine($"Found Assembly \"{name}\" at {DATA:X8} <{PREV:X8}|{NEXT:X8}>");
				Console.WriteLine($"With MonoImage at {MonoImage:X8}");
				String fileName = ReadNTStringAt(MonoImage + 0x14);
				String assemblyName = ReadNTStringAt(MonoImage + 0x18);
				String moduleName = ReadNTStringAt(MonoImage + 0x1C);
				String version = ReadNTStringAt(MonoImage + 0x20);
				String guid = ReadNTStringAt(MonoImage + 0x28);
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
					String className = ReadNTString(HeapString + row[1]);
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
				String classname = ReadNTStringAt(MonoClass + 0x2C);
				String classnamespace = ReadNTStringAt(MonoClass + 0x30);
				Int32 ClassType = Read<Int32>(MonoClass + 0x34);
				Console.WriteLine($"Class {classnamespace} . {classname} ({TabletopManagerTypeToken:X8} = {ClassType:X8}) at {MonoClass:X8}");
			}

			//Int32 nameAddress = FindBytePattern(_process, 0, 0x21000000, new Byte?[] { 0x41, 0x73, 0x73, 0x65, 0x74, 0x73, 0x2E, 0x43, 0x53, 0x2E, 0x54, 0x61, 0x62, 0x6C, 0x65, 0x74, 0x6F, 0x70, 0x55, 0x49, 0x00 }).First();
			//Console.WriteLine($"Found name at {nameAddress:X8}");
			//Int32 namespaceAddress;



			return 0;
		}

		public Int32 FindSituationsCatalogue ( ) {
			return 0;
		}

		public T Read<T> ( Int32 address ) where T : struct {
			return Read<T>(_process, address);
		}
		public String ReadNTString ( Int32 address ) {
			return ReadNTString(_process, address);
		}
		public String ReadNTStringAt ( Int32 address ) {
			return ReadNTStringAt(_process, address);
		}
		public String ReadString ( Int32 address ) {
			return ReadString(_process, address);
		}
	}
}