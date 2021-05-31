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
			
			public Int32? FindVTableOfClassInClassCache ( Int32 image, String name ) {
				Int32 class_cache_size = Read<Int32>(image + 0x360);
				Int32 class_cache_table = Read<Int32>(image + 0x368);
				for ( Int32 i = 0 ; i < class_cache_size ; ++i ) {
					Int32 pointer = Read<Int32>(class_cache_table + i * 4);
					if ( pointer == 0 )
						continue;

					Int32 klass = Read<Int32>(pointer);
					String classname = ReadUTF8StringAt(klass + 0x2C);

					if ( !classname.Equals(name) )
						continue;

					Int32 MonoClassRuntimeInfo = Read<Int32>(klass + 0x84);
					Int32 MonoVTable = Read<Int32>(MonoClassRuntimeInfo + 0x04);
					return MonoVTable;
					// pointer = Read<Int32>(pointer + 0xA8);
				}
				return null;
			}
			public List<Int32> EnumImageClassCache ( Int32 image ) {
				List<Int32> entries = new List<Int32>();

				Int32 class_cache_size = Read<Int32>(image + 0x360);
				Int32 class_cache_table = Read<Int32>(image + 0x368);
				for ( Int32 i = 0 ; i < class_cache_size ; ++i ) {
					Int32 pointer = Read<Int32>(class_cache_table + i * 4);
					if ( pointer != 0 ) {
						Int32 klass = Read<Int32>(pointer);
						String name = ReadUTF8StringAt(klass + 0x2C);
						String name_space = ReadUTF8StringAt(klass + 0x30);
						Int32 type_token = Read<Int32>(klass + 0x34);
						Int32 MonoClassFieldArray = Read<Int32>(klass + 0x60);
						Int32 MonoMethodArray = Read<Int32>(klass + 0x64);
						Int32 MonoClassRuntimeInfo = Read<Int32>(klass + 0x84);
						Int32 MonoVTable = Read<Int32>(MonoClassRuntimeInfo + 0x04);
						Console.WriteLine($"Found class {type_token:X8}:\"{name_space}.{name}\" ({klass:X8}) with Vtable at {MonoVTable:X8}");
						entries.Add(klass);
						// pointer = Read<Int32>(pointer + 0xA8);
					}
				}
				Console.WriteLine($"Found {entries.Count} entries");
				return entries;
			}
			public Int32? GetClassFromImageCache ( Int32 image, Int32 type_token ) {
				Int32 class_cache_size = Read<Int32>(image + 0x360);
				Int32 class_cache_table = Read<Int32>(image + 0x368);

				Int32 pointer = Read<Int32>(class_cache_table + (type_token % class_cache_size) * 4);
				if ( pointer != 0 ) {
					Int32 klass = Read<Int32>(pointer);
					if ( Read<Int32>(klass + 0x34) == type_token )
						return klass;
					// pointer = Read<Int32>(pointer + 0xA8);
				}

				return null;
			}
		#endregion

		public List<Int32> FindInstances( Int32 vtable ) {
			return FindBytePattern(0, 0x23000000, new Byte?[] {
				(Byte) vtable,
				(Byte) (vtable >> 8),
				(Byte) (vtable >> 16),
				(Byte) (vtable >> 24),
				0x00,
				0x00,
				0x00,
				0x00
			});
		}

		public Int32? FindVTable ( Int32 domain, Int32 klass ) {
			Byte?[] pattern = new Byte?[] {
				(Byte) klass, (Byte) (klass >> 8), (Byte) (klass >> 16), (Byte) (klass >> 24),
				null, null, null, null, // GC description
				(Byte) domain, (Byte) (domain >> 8), (Byte) (domain >> 16), (Byte) (domain >> 24)
			};
			List<Int32> matches = FindBytePattern(0, 0x23000000, pattern);
			Console.WriteLine($"Found {matches.Count} matches");
			return matches.FirstOrDefault();
		}

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
