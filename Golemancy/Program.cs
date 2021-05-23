using System;
using System.Collections.Generic;
using System.Linq;
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

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Overlay());
		}
	}
}
