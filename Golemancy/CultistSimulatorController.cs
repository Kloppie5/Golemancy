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

		public Int32? GetClassFromImageCache ( Int32 image, Int32 type_token ) {
			Int32 class_cache_size = Read<Int32>(image + 0x360);
			Int32 class_cache_table = Read<Int32>(image + 0x368);

			Int32 pointer = Read<Int32>(class_cache_table + (type_token % class_cache_size) * 4);
			while ( pointer != 0 ) {
				Int32 klass = Read<Int32>(pointer);
				if ( Read<Int32>(klass + 0x34) == type_token )
					return klass;
				pointer = Read<Int32>(pointer + 0xA8);
			}

			return null;
		}

		public Int32 FindTabletopManager ( ) {
			Int32 UnityRootDomain = GetUnityRootDomain();
			Console.WriteLine($"Found Domain at {UnityRootDomain:X8}");
			Int32 assembly = FindAssemblyInDomain(UnityRootDomain, "Assembly-CSharp").Value;
			Console.WriteLine($"Found Assembly at {assembly:X8}");
			Int32 image = Read<Int32>(assembly + 0x44);
			Console.WriteLine($"Found Image at {image:X8}");
			Int32 index = FindTYPEDEFMetaTableIndexInImage(image, "TabletopManager").Value;
			Console.WriteLine($"Found TabletopManager index {index}");
			Int32 type_token = 0x02000000 | (index + 1);
			Int32 klass = GetClassFromImageCache(image, type_token).Value;
			Console.WriteLine($"Found Class at {klass:X8}");
			Int32 vtable = FindVTable(UnityRootDomain, klass).Value;
			Console.WriteLine($"Found VTable at {vtable:X8}");

			List<Int32> matches = FindInstances( vtable );
			List<Int32> instances = new List<Int32>();
			foreach ( Int32 match in matches ) {
				Int32 cachedPtr = Read<Int32>(match + 0x8);
				if ( cachedPtr != 0 )
					instances.Add(match);
			}

			Console.WriteLine($"Found {instances.Count} instances");

			return instances.First();
		}

		public Int32 FindSituationsCatalogue ( ) {
			Int32 UnityRootDomain = GetUnityRootDomain();
			Console.WriteLine($"Found Domain at {UnityRootDomain:X8}");
			Int32 assembly = FindAssemblyInDomain(UnityRootDomain, "Assembly-CSharp").Value;
			Console.WriteLine($"Found Assembly at {assembly:X8}");
			Int32 image = Read<Int32>(assembly + 0x44);
			Console.WriteLine($"Found Image at {image:X8}");
			Int32 index = FindTYPEDEFMetaTableIndexInImage(image, "SituationsCatalogue").Value;
			Console.WriteLine($"Found SituationsCatalogue index {index}");
			Int32 type_token = 0x02000000 | (index + 1);
			Int32 klass = GetClassFromImageCache(image, type_token).Value;
			Console.WriteLine($"Found Class at {klass:X8}");
			Int32 vtable = FindVTable(UnityRootDomain, klass).Value;
			Console.WriteLine($"Found VTable at {vtable:X8}");

			List<Int32> matches = FindInstances( vtable );
			List<Int32> instances = new List<Int32>();
			foreach ( Int32 match in matches ) {
				Int32 currentSituationControllers = Read<Int32>(match + 0x8);
				if ( currentSituationControllers != 0 )
					instances.Add(match);
			}

			Console.WriteLine($"Found {instances.Count} instances");

			return instances.First();
		}
	}
}
