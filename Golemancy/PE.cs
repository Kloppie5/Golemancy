using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Sequential)]
public struct IMAGE_DATA_DIRECTORY {
    public uint VirtualAddress;
    public uint Size;
}
public struct IMAGE_DOS_HEADER {        // DOS .EXE header
    public ushort e_magic;              // Magic number
    public ushort e_cblp;               // Bytes on last page of file
    public ushort e_cp;                 // Pages in file
    public ushort e_crlc;               // Relocations
    public ushort e_cparhdr;            // Size of header in paragraphs
    public ushort e_minalloc;           // Minimum extra paragraphs needed
    public ushort e_maxalloc;           // Maximum extra paragraphs needed
    public ushort e_ss;                 // Initial (relative) SS value
    public ushort e_sp;                 // Initial SP value
    public ushort e_csum;               // Checksum
    public ushort e_ip;                 // Initial IP value
    public ushort e_cs;                 // Initial (relative) CS value
    public ushort e_lfarlc;             // File address of relocation table
    public ushort e_ovno;               // Overlay number
    public ushort e_res_0;              // Reserved words
    public ushort e_res_1;              // Reserved words
    public ushort e_res_2;              // Reserved words
    public ushort e_res_3;              // Reserved words
    public ushort e_oemid;              // OEM identifier (for e_oeminfo)
    public ushort e_oeminfo;            // OEM information; e_oemid specific
    public ushort e_res2_0;             // Reserved words
    public ushort e_res2_1;             // Reserved words
    public ushort e_res2_2;             // Reserved words
    public ushort e_res2_3;             // Reserved words
    public ushort e_res2_4;             // Reserved words
    public ushort e_res2_5;             // Reserved words
    public ushort e_res2_6;             // Reserved words
    public ushort e_res2_7;             // Reserved words
    public ushort e_res2_8;             // Reserved words
    public ushort e_res2_9;             // Reserved words
    public uint e_lfanew;             // File address of new exe header
}
[StructLayout(LayoutKind.Sequential)]
public struct IMAGE_EXPORT_DIRECTORY_TABLE {
    public uint ExportFlags;
    public uint TimeDateStamp;
    public ushort MajorVersion;
    public ushort MinorVersion;
    public uint NameRva;
    public uint OrdinalBase;
    public uint AddressTableEntries;
    public uint NumberOfNamePointers;
    public uint ExportAddressTableRva;
    public uint NamePointerRva;
    public uint OrdinalTableRva;
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_OPTIONAL_HEADER32 {
    public ushort Magic;
    public byte MajorLinkerVersion;
    public byte MinorLinkerVersion;
    public uint SizeOfCode;
    public uint SizeOfInitializedData;
    public uint SizeOfUninitializedData;
    public uint AddressOfEntryPoint;
    public uint BaseOfCode;
    public uint BaseOfData;
    public uint ImageBase;
    public uint SectionAlignment;
    public uint FileAlignment;
    public ushort MajorOperatingSystemVersion;
    public ushort MinorOperatingSystemVersion;
    public ushort MajorImageVersion;
    public ushort MinorImageVersion;
    public ushort MajorSubsystemVersion;
    public ushort MinorSubsystemVersion;
    public uint Win32VersionValue;
    public uint SizeOfImage;
    public uint SizeOfHeaders;
    public uint CheckSum;
    public ushort Subsystem;
    public ushort DllCharacteristics;
    public uint SizeOfStackReserve;
    public uint SizeOfStackCommit;
    public uint SizeOfHeapReserve;
    public uint SizeOfHeapCommit;
    public uint LoaderFlags;
    public uint NumberOfRvaAndSizes;

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