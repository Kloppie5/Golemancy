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

		/**
			A TabletopTokenContainer has a Choreographer
			A Choreographer manages automated card moves such as recipe results and greedy slots.

		*/
		#region TabletopManager
			public Int32 FindTabletopManager ( Int32 UnityRootDomain, Int32 image ) {
				Int32 vtable = FindVTableOfClassInClassCache(image, "TabletopManager").Value;

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
			public void DBPTabletopManager ( Int32 UnityRootDomain, Int32 image ) {
				Int32 TabletopManager = FindTabletopManager( UnityRootDomain, image );
				Console.WriteLine($"Found TabletopManager at {TabletopManager:X8}");
				Int32 TabletopTokenContainer = Read<Int32>(TabletopManager + 0x10);
				Console.WriteLine($"> 0x10: TabletopTokenContainer ({TabletopTokenContainer:X8})");
				Int32 ElementStacksManager = Read<Int32>(TabletopTokenContainer + 0xC);
				Console.WriteLine($">> 0x0C: ElementStacksManager ({ElementStacksManager:X8})");
				Int32 TokenContainer = Read<Int32>(ElementStacksManager + 0x8);
				Console.WriteLine($">>> 0x08: TokenContainer ({TokenContainer:X8})");
				Int32 ElementStackTokenList = Read<Int32>(ElementStacksManager + 0xC);
				Console.WriteLine($">>> 0x0C: ElementStackTokenList ({ElementStackTokenList:X8})");
				Int32 array = Read<Int32>(ElementStackTokenList + 0x8);
				Int32 count = Read<Int32>(ElementStackTokenList + 0xC);
				Int32 capacity = Read<Int32>(ElementStackTokenList + 0x10);
				Console.WriteLine($">>>> 0x08: Backing array [{count}/{capacity}] ({array:X8})");

				for ( Int32 i = 0 ; i < count ; ++i ) {
					Int32 ElementStackToken = Read<Int32>(array + 0x10 + i * 0x4);
					Int32 Element = Read<Int32>(ElementStackToken + 0xB0);
					String id = ReadUnicodeStringAt(Element + 0x8);
					String Label_BackingField = ReadUnicodeStringAt(Element + 0x1C);
					Int32 quantity = Read<Int32>(ElementStackToken + 0xE4);
					Console.WriteLine($">>>>> {quantity}x {id}:\"{Label_BackingField}\" ({Element:X8})");
				}
				// Int32 StackManagersCatalogue = Read<Int32>(ElementStacksManager + 0x10);
				// Int32 Coreographer = Read<Int32>(TabletopTokenContainer + 0x1C);
				// Int32 AspectDetailsWindow = Read<Int32>(TabletopManager + 0x1C);
				// Int32 TokenDetailsWindow = Read<Int32>(TabletopManager + 0x20);
				// Int32 CardHoverDetail = Read<Int32>(TabletopManager + 0x24);
				// Int32 MapController = Read<Int32>(TabletopManager + 0x28);
				// Int32 MapTokenContainer = Read<Int32>(TabletopManager + 0x2C);
				// Int32 StatusBar = Read<Int32>(TabletopManager + 0x4C);
				// Int32 SituationBuilder = Read<Int32>(TabletopManager + 0x60);
				// Single housekeepingTimer = Read<Single>(TabletopManager + 0x7C);
			}
		#endregion
		#region SituationsCatalogue
		/**
			A SituationsCatalogue is a registry contained object with links to the different SituationController's

			A SituationController has;
			- SituationToken : ISituationAnchor determining its position
			- SituationWindow : ISituationDetails
			- ISituationView
			- ISituationStorage
			- ISituationClock
			- ICompendium
		*/
			public Int32 FindSituationsCatalogue ( Int32 UnityRootDomain, Int32 image ) {
					Int32 vtable = FindVTableOfClassInClassCache(image, "SituationsCatalogue").Value;

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
			public void DBPSituationsCatalogue ( Int32 UnityRootDomain, Int32 image ) {
				Int32 SituationsCatalogue = FindSituationsCatalogue(UnityRootDomain, image);
				Console.WriteLine($"Found SituationsCatalogue at {SituationsCatalogue:X8}");
				Int32 SituationControllerList = Read<Int32>(SituationsCatalogue + 0x8);
				Console.WriteLine($"> 0x08: SituationsCatalogue ({SituationsCatalogue:X8})");
				Int32 array = Read<Int32>(SituationControllerList + 0x8);
				Int32 count = Read<Int32>(SituationControllerList + 0xC);
				Int32 capacity = Read<Int32>(SituationControllerList + 0x10);
				Console.WriteLine($">> 0x08: Backing array [{count}/{capacity}] ({array:X8})");

				for ( Int32 i = 0 ; i < count ; ++i ) {
					Int32 SituationController = Read<Int32>(array + 0x10 + i * 0x4);
					Int32 SituationToken = Read<Int32>(SituationController + 0x8);
					Int32 verb = Read<Int32>(SituationToken + 0xB8);
					String id = ReadUnicodeStringAt(verb + 0x8);
					String Label_BackingField = ReadUnicodeStringAt(verb + 0x1C);
					Console.WriteLine($">>> {id}:\"{Label_BackingField}\" ({SituationToken:X8})");
				}		
			}
		#endregion
	}
}
