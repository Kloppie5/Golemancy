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
            Console.WriteLine($"Linked to \"{process.ProcessName}\"");

            CultistSimulatorController CSC = new CultistSimulatorController(process);

            Int32 UnityRootDomain = CSC.GetUnityRootDomain();
			Console.WriteLine($"Found Domain at {UnityRootDomain:X8}");
			Int32 assembly = CSC.FindAssemblyInDomain(UnityRootDomain, "Assembly-CSharp").Value;
			Console.WriteLine($"Found Assembly at {assembly:X8}");
			Int32 image = CSC.Read<Int32>(assembly + 0x44);
			Console.WriteLine($"Found Image at {image:X8}");

            CSC.EnumImageClassCache(image);

            CSC.DBPTabletopManager(UnityRootDomain, image);
            CSC.DBPSituationsCatalogue(UnityRootDomain, image);

            // work legacybytjob => legacybytjobmatured, 3 funds, explore {some action => locationauctionhouse, locationcabaret}
            // work legacybytjobmatured => [(legacybytjobmatured, 3 funds), (2 funds, legacyeventbad)]
            // work health => fatigue, vitality, funds
            // legacyeventbad => 2 health, 20 funds, study {some action => legacydiarylastcharacter, vienneseconundra, dream {some action => passion, influencewinter} }
            // time, funds
            
            // dream passion => passionexhausted, contentment
            // dream injury, funds => health
            // dream ascensionsentationa, fragmentgrail => fragmentgrail, ascensionsensationbf
            // dream affliction, funds => health, heartinduction

            // heartinduction => vitality
            // mothinduction => restlessness

            // explore fleeting => fascination

            // study 2x vitality => health, skillhealtha
            // study textbooklatin => scholarlatin
            // study scholarlatin, orchidtransfigurations2latin => scholarlatin, orchidtransfigurations2
            // study vienneseconundra => fragmentmothc, glimmering
            // study orchidtransfigurations2 => fragmentgrailb
            // study textbookreason => eruditionplus
            // study legacydiarylastcharacter => reason, fragmentgrail
            // study textbookgreek => scholargreek
            // study locksmithsdream2 => fragmentknock, erudition
            // study scholarlatin, dehoris1latin => scholarlatin, dehoris1
            // study skeletonsongs => fragmentgrail
            // study erudition, erudition => skillreasona, reason
            // study fragmentgrail, fragmentgrail

            // ? => talk, poppyready, ascensionsensationa
            
            // poppytime => DEATH

            // influencewinter DECAYEDTO dread
            // restlessness DECAYEDTO dread
            
            // explore health => [({option to hire}, fatigue), (locationbookdealer, fatigue)]
            
            // work skillhealtha, health => skillhealtha, injury, funds
            // work passion => passionexhausted, glimmering
            // work reason => reason, gloverandgloverjuniorjob
            // work gloverandgloverjuniorjob => gloverandgloverjuniorjob, funds
            // work gloverandgloverjuniorjob, reason => gloverandglover_difficultbossa_job, concentration, funds
            // work gloverandglover_difficultbossa_job, reason => gloverandglover_difficultbossa_job, concentration, 2 funds

            // talk poppyready, passion => passion, 10 funds, compensationc, poppytime, mothinduction

            // suspicion => contentment
            // visions => fleeting
            // despair => fleeting
            // craving => restlessness
            // rose => contentment
            // illhealth health => affliction

            /** explore locationauctionhouse => [
                textbookreason
                waroftheroadcensored
                orchidtransfigurations2latin
                textbooklatin
                apolloandmarsyas
                introductiontohistories
                truecompleteasclepiangreek
                locksmithsdream2
                textbookgreek
                dehoris1latin
                skeletonsongs
                humoursofagentleman
                orchidtransfigurations1latin
                stumm
                travellingatnight2
                sixlettersonnecessity
                onthewhite
            ]*/

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
