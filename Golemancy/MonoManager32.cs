using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golemancy {
	class MonoManager32 : MemoryManager32 {
		/**
			Supposed IntPtrs are replaced with Int32 to not cause an enormous headache.
		*/

		public MonoManager32( Process process ) : base(process) {
			//GetModuleBaseAddress(_process, "mono-2.0-bdwgc.dll");
		}

		public Int32 GetUnityRootDomain() {
			Int32 MonoBaseAddress = GetModuleBaseAddress("mono-2.0-bdwgc.dll");
			return Read<Int32>(MonoBaseAddress + 0x3A41AC);
		}

		#region MonoDomain
			/**
			 * Traverses the MonoDomain for the MonoAssembly with the given name.
			 */
			public Int32? FindAssemblyInDomain ( Int32 domain, String name ) {
				Int32 it = Read<Int32>(domain + 0x6C);
				while ( it != 0 ) {
					Int32 assembly = Read<Int32>(it);
					String assemblyName = ReadUTF8StringAt(assembly + 0x8);

					if ( assemblyName.Equals(name) )
						return assembly;

					it = Read<Int32>(it + 0x4);
				}
				return null;
			}

			/**
			 * A MonoDomain object has a linked list containing MonoAssembly objects.
			 * The github says it is a GSList, but direct observation clearly shows it is doubly linked.
			 */
			public List<Int32> EnumAssemblyInDomain ( Int32 domain ) {
				List<Int32> entries = new List<Int32>();

				Int32 it = Read<Int32>(domain + 0x6C);
				while ( it != 0 ) {
					entries.Add(Read<Int32>(it));
					it = Read<Int32>(it + 0x4);
				}
	
				return entries;
			}
		#endregion
		#region MonoImage
			public Int32? FindTYPEDEFMetaTableIndexInImage ( Int32 image, String name ) {
				Int32 heap_strings = Read<Int32>(image + 0x38);
				Int32 tableBase = Read<Int32>(image + 0x94);
				Int32 dimRows = Read<Int32>(image + 0x98);
				Int32 rowCount = dimRows & 0xFFFFFF;
				Int32 rowSize = dimRows >> 24;
				Int32 dimCols = Read<Int32>(image + 0x9C);
				Int32 colCount = dimCols >> 24;

				for ( Int32 i = 0 ; i < rowCount ; ++i ) {
					Int32 pointer = tableBase + i * rowSize;
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
					// {FLAGS, NAME, NAMESPACE, EXTENDS, FIELD_LIST, METHOD_LIST}
					String className = ReadUTF8String(heap_strings + row[1]);
					if ( className.Equals(name) )
						return i;
				}

				return null;
			}

			public List<String> EnumTYPEDEFMetaTableNamesInImage ( Int32 image ) {
				List<String> entries = new List<String>();

				Int32 heap_strings = Read<Int32>(image + 0x38);
				Int32 tableBase = Read<Int32>(image + 0x94);
				Int32 dimRows = Read<Int32>(image + 0x98);
				Int32 rowCount = dimRows & 0xFFFFFF;
				Int32 rowSize = dimRows >> 24;
				Int32 dimCols = Read<Int32>(image + 0x9C);
				Int32 colCount = dimCols >> 24;

				for ( Int32 i = 0 ; i < rowCount ; ++i ) {
					Int32 pointer = tableBase + i * rowSize;
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
					// {FLAGS, NAME, NAMESPACE, EXTENDS, FIELD_LIST, METHOD_LIST}
					String className = ReadUTF8String(heap_strings + row[1]);
					entries.Add(className);
				}

				return entries;
			}
		#endregion

		/**
			* Mono changes String objects following the MonoObject design;
			* They now start with a MonoVTable field and a MonoThreadsSync field, so size and data fields are placed 0x4 further in
			*/	
		new public String ReadUnicodeStringAt ( Int32 address ) {
			Int32 stringAddress = Read<Int32>(address);
			Int32 size = Read<Int32>(stringAddress + 0x8);

			return ReadUnicodeString(stringAddress + 0xC, size);
		}
		
	}
}
