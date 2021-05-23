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
