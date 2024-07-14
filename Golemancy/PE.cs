using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Sequential)]
public struct IMAGE_DATA_DIRECTORY {
    public UInt32 VirtualAddress;
    public UInt32 Size;
}
public struct IMAGE_DOS_HEADER {        // DOS .EXE header
    public UInt16 e_magic;              // Magic number
    public UInt16 e_cblp;               // Bytes on last page of file
    public UInt16 e_cp;                 // Pages in file
    public UInt16 e_crlc;               // Relocations
    public UInt16 e_cparhdr;            // Size of header in paragraphs
    public UInt16 e_minalloc;           // Minimum extra paragraphs needed
    public UInt16 e_maxalloc;           // Maximum extra paragraphs needed
    public UInt16 e_ss;                 // Initial (relative) SS value
    public UInt16 e_sp;                 // Initial SP value
    public UInt16 e_csum;               // Checksum
    public UInt16 e_ip;                 // Initial IP value
    public UInt16 e_cs;                 // Initial (relative) CS value
    public UInt16 e_lfarlc;             // File address of relocation table
    public UInt16 e_ovno;               // Overlay number
    public UInt16 e_res_0;              // Reserved words
    public UInt16 e_res_1;              // Reserved words
    public UInt16 e_res_2;              // Reserved words
    public UInt16 e_res_3;              // Reserved words
    public UInt16 e_oemid;              // OEM identifier (for e_oeminfo)
    public UInt16 e_oeminfo;            // OEM information; e_oemid specific
    public UInt16 e_res2_0;             // Reserved words
    public UInt16 e_res2_1;             // Reserved words
    public UInt16 e_res2_2;             // Reserved words
    public UInt16 e_res2_3;             // Reserved words
    public UInt16 e_res2_4;             // Reserved words
    public UInt16 e_res2_5;             // Reserved words
    public UInt16 e_res2_6;             // Reserved words
    public UInt16 e_res2_7;             // Reserved words
    public UInt16 e_res2_8;             // Reserved words
    public UInt16 e_res2_9;             // Reserved words
    public UInt32 e_lfanew;             // File address of new exe header
}
[StructLayout(LayoutKind.Sequential)]
public struct IMAGE_EXPORT_DIRECTORY_TABLE {
    public UInt32 ExportFlags;
    public UInt32 TimeDateStamp;
    public UInt16 MajorVersion;
    public UInt16 MinorVersion;
    public UInt32 NameRva;
    public UInt32 OrdinalBase;
    public UInt32 AddressTableEntries;
    public UInt32 NumberOfNamePointers;
    public UInt32 ExportAddressTableRva;
    public UInt32 NamePointerRva;
    public UInt32 OrdinalTableRva;
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_OPTIONAL_HEADER32 {
    public UInt16 Magic;
    public Byte MajorLinkerVersion;
    public Byte MinorLinkerVersion;
    public UInt32 SizeOfCode;
    public UInt32 SizeOfInitializedData;
    public UInt32 SizeOfUninitializedData;
    public UInt32 AddressOfEntryPoint;
    public UInt32 BaseOfCode;
    public UInt32 BaseOfData;
    public UInt32 ImageBase;
    public UInt32 SectionAlignment;
    public UInt32 FileAlignment;
    public UInt16 MajorOperatingSystemVersion;
    public UInt16 MinorOperatingSystemVersion;
    public UInt16 MajorImageVersion;
    public UInt16 MinorImageVersion;
    public UInt16 MajorSubsystemVersion;
    public UInt16 MinorSubsystemVersion;
    public UInt32 Win32VersionValue;
    public UInt32 SizeOfImage;
    public UInt32 SizeOfHeaders;
    public UInt32 CheckSum;
    public UInt16 Subsystem;
    public UInt16 DllCharacteristics;
    public UInt32 SizeOfStackReserve;
    public UInt32 SizeOfStackCommit;
    public UInt32 SizeOfHeapReserve;
    public UInt32 SizeOfHeapCommit;
    public UInt32 LoaderFlags;
    public UInt32 NumberOfRvaAndSizes;

    public IMAGE_DATA_DIRECTORY ExportTable;
    public IMAGE_DATA_DIRECTORY ImportTable;
    public IMAGE_DATA_DIRECTORY ResourceTable;
    public IMAGE_DATA_DIRECTORY ExceptionTable;
    public IMAGE_DATA_DIRECTORY CertificateTable;
    public IMAGE_DATA_DIRECTORY BaseRelocationTable;
    public IMAGE_DATA_DIRECTORY Debug;
    public IMAGE_DATA_DIRECTORY Architecture;
    public IMAGE_DATA_DIRECTORY GlobalPtr;
    public IMAGE_DATA_DIRECTORY TLSTable;
    public IMAGE_DATA_DIRECTORY LoadConfigTable;
    public IMAGE_DATA_DIRECTORY BoundImport;
    public IMAGE_DATA_DIRECTORY IAT;
    public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
    public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
    public IMAGE_DATA_DIRECTORY Reserved;
}