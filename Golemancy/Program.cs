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

            CultistSimulatorController csc = new CultistSimulatorController(process);

            Console.ReadLine();

            Int32 TabletopManager = csc.FindTabletopManager();
            Console.WriteLine($"Found TabletopManager at {TabletopManager:X8}");
                Int32 tttc = MemoryManager32.Read<Int32>(process, TabletopManager + 0x10);
                    Int32 esm = MemoryManager32.Read<Int32>(process, tttc + 0xC);
                        Int32 tc = MemoryManager32.Read<Int32>(process, esm + 0x8);
                        Int32 stacks = MemoryManager32.Read<Int32>(process, esm + 0xC);
                            {
                                Int32 array = MemoryManager32.Read<Int32>(process, stacks + 0x8);
                                Int32 count = MemoryManager32.Read<Int32>(process, stacks + 0xC);
                                Int32 capacity = MemoryManager32.Read<Int32>(process, stacks + 0x10);

                                for ( Int32 i = 0 ; i < count ; ++i ) {
                                    Int32 elementi = MemoryManager32.Read<Int32>(process, array + 0x10 + i * 0x4);

                                    Int32 element = MemoryManager32.Read<Int32>(process, elementi + 0xB0);
                                        String elementID = MemoryManager32.ReadString(process, element + 0x8);
                                        String elementLabel = MemoryManager32.ReadString(process, element + 0x1C);
                                    Int32 quantity = MemoryManager32.Read<Int32>(process, elementi + 0xE4);
                                    Console.WriteLine($"- {quantity}x \"{elementLabel}\" ({elementID})");
                                    if (elementID == "funds") Console.WriteLine($"{elementi + 0xE4:X8}");
                                }
                            }
                        Int32 smc = MemoryManager32.Read<Int32>(process, esm + 0x10);
                    Int32 c = MemoryManager32.Read<Int32>(process, tttc + 0x1C);

                Int32 adw = MemoryManager32.Read<Int32>(process, TabletopManager + 0x1C);
                Int32 tdw = MemoryManager32.Read<Int32>(process, TabletopManager + 0x20);
                Int32 mc = MemoryManager32.Read<Int32>(process, TabletopManager + 0x28);
                Int32 mtc = MemoryManager32.Read<Int32>(process, TabletopManager + 0x2C);
                Int32 sb = MemoryManager32.Read<Int32>(process, TabletopManager + 0x4C);
                Single hkt = MemoryManager32.Read<Single>(process, TabletopManager + 0x7C);

            Int32 sc = csc.FindSituationsCatalogue();
            Console.WriteLine($"Found SituationsCatalogue at {sc:X8}");
                Int32 SituationControllerList = MemoryManager32.Read<Int32>(process, sc + 0x8);
                    {
                        Int32 array = MemoryManager32.Read<Int32>(process, SituationControllerList + 0x8);
                        Int32 count = MemoryManager32.Read<Int32>(process, SituationControllerList + 0xC);
                        Int32 capacity = MemoryManager32.Read<Int32>(process, SituationControllerList + 0x10);

                        for ( Int32 i = 0 ; i < count ; ++i ) {
                            Int32 situationController = MemoryManager32.Read<Int32>(process, array + 0x10 + i * 0x4);
                                Int32 situationToken = MemoryManager32.Read<Int32>(process, situationController + 0x8);
                                    Int32 verb = MemoryManager32.Read<Int32>(process, situationToken + 0xB8);
                                        String verbID = MemoryManager32.ReadString(process, verb + 0x8);
                                        String verbLabel = MemoryManager32.ReadString(process, verb + 0x1C);
                                        Console.WriteLine($"- \"{verbLabel}\" ({verbID})");
                        }
                    }

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
