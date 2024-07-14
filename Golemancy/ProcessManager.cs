using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Golemancy;

class ProcessManager {

    [DllImport("psapi.dll")]
    public static extern bool EnumProcessModulesEx ( nint hProcess, [Out] nint[] lphModule, int cb, ref int lpcbNeeded, int dwFilterFlag );
    [DllImport("psapi.dll")]
    public static extern Int32 GetModuleFileNameEx( nint hProcess, nint hModule, [Out] StringBuilder lpBaseName, int nSize );

    public static IEnumerable<nint> GetModules64ByName( nint hProcess, string name ) {
      Dictionary<string, nint> modules = new Dictionary<string, nint>();
      nint[] hModules = new nint[1024];
      int lpcbNeeded = 0;
      EnumProcessModulesEx(hProcess, hModules, 1024, ref lpcbNeeded, 0x3);
      for ( int i = 0; i < lpcbNeeded / IntPtr.Size; i++ ) {
        StringBuilder sb = new(1024);
        GetModuleFileNameEx(hProcess, hModules[i], sb, 1024);
        if ( sb.ToString().Contains(name) )
          yield return hModules[i];
      }
    }

}