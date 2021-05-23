using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Golemancy {
	partial class METADATA {
		public static String Version = "0.000.001";

		public static String AppID = "718670";
		public static String BuildID = "6212706";
	}	

	static class Program {
		[STAThread]
		static void Main() {
			Console.WriteLine($"Initializing Golemancy v{METADATA.Version} for Cultist Simulator, build {METADATA.BuildID}");

			Process process = Process.GetProcessesByName("cultistsimulator").FirstOrDefault();
            Console.WriteLine("process: " + process);

            IntPtr processHandle = process.Handle;
            Console.WriteLine("Process Handle: " + processHandle);

            CultistSimulatorMemoryManager csmm = new CultistSimulatorMemoryManager();
            UInt32 ttm = csmm.FindTabletopManager(process);
            Console.WriteLine($"Found TabletopManager at {ttm:X8}");

                UInt32 tttc = CultistSimulatorMemoryManager.Read<UInt32>(process, ttm + 0x10);
                Console.WriteLine($"Found TabletopTokenContainer at {tttc:X8}");

                    UInt32 esm = CultistSimulatorMemoryManager.Read<UInt32>(process, tttc + 0xC);
                    Console.WriteLine($"Found ElementStacksManager at {esm:X8}");

                        UInt32 tc = CultistSimulatorMemoryManager.Read<UInt32>(process, esm + 0x8);
                        Console.WriteLine($"Found TokenContainer at {tc:X8}");
                        
                        UInt32 stacks = CultistSimulatorMemoryManager.Read<UInt32>(process, esm + 0xC);
                        Console.WriteLine($"Found List<ElementStackToken> at {stacks:X8}");
                            UInt32 array = CultistSimulatorMemoryManager.Read<UInt32>(process, stacks + 0x8);
                            UInt32 count = CultistSimulatorMemoryManager.Read<UInt32>(process, stacks + 0xC);
                            UInt32 capacity = CultistSimulatorMemoryManager.Read<UInt32>(process, stacks + 0x10);
                            Console.WriteLine($"Supporting array ({count}/{capacity}) at {array:X8}");

                            for ( UInt32 i = 0 ; i < count ; ++i ) {
                                UInt32 elementi = CultistSimulatorMemoryManager.Read<UInt32>(process, array + 0x10 + i * 0x4);

                                UInt32 element = CultistSimulatorMemoryManager.Read<UInt32>(process, elementi + 0xB0);
                                    String elementID = CultistSimulatorMemoryManager.ReadString(process, element + 0x8);
                                    String elementLabel = CultistSimulatorMemoryManager.ReadString(process, element + 0x1C);
                                Int32 quantity = CultistSimulatorMemoryManager.Read<Int32>(process, elementi + 0xE4);
                                Console.WriteLine($"- {quantity}x \"{elementLabel}\" ({elementID})");
                            }
                        UInt32 smc = CultistSimulatorMemoryManager.Read<UInt32>(process, esm + 0x10);
                        Console.WriteLine($"Found StackManagersCatalogue at {smc:X8}");

                UInt32 adw = CultistSimulatorMemoryManager.Read<UInt32>(process, ttm + 0x1C);
                Console.WriteLine($"Found AspectDetailsWindow at {adw:X8}");

                UInt32 tdw = CultistSimulatorMemoryManager.Read<UInt32>(process, ttm + 0x20);
                Console.WriteLine($"Found TokenDetailsWindow at {tdw:X8}");

                UInt32 mc = CultistSimulatorMemoryManager.Read<UInt32>(process, ttm + 0x28);
                Console.WriteLine($"Found MapController at {mc:X8}");

                UInt32 sb = CultistSimulatorMemoryManager.Read<UInt32>(process, ttm + 0x4C);
                Console.WriteLine($"Found StatusBar at {sb:X8}");

                Single hkt = CultistSimulatorMemoryManager.Read<Single>(process, ttm + 0x7C);
                Console.WriteLine($"Found housekeepingTimer at {hkt}");



            Console.ReadLine();

            /**
            
            Assets.CS.TabletopUI.Registry
            
            
            A Situation is wrapped in a SituationController

            A SituationController has a link back to the current Character

            TabletopTokenContainer
            contains Choreagrapher
            Token dictionary

            TabletopManager
            contains TabletopTokenContainer
            contains MapController
             */

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Overlay());
		}
	}
}
