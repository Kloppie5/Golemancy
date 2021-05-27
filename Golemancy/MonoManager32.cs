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

		// Mono changes String objects following the MonoObject design of starting with a MonoVTable field and a MonoThreadsSync field, so size and data are placed 0x4 further
		new public String ReadUnicodeStringAt ( Int32 address ) {
			Int32 stringAddress = Read<Int32>(address);
			Int32 size = Read<Int32>(stringAddress + 0x8);

			return ReadUnicodeString(stringAddress + 0xC, size);
		}
	}
}
