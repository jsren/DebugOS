
namespace DebugOS.COFF
{
    public enum StorageClass
    {
        None             = 0x00,
        Automatic,
        External,
        Static,
        Register,
        ExternalDef,
        Label,
        UndefinedLabel,
        StructMember,
        Argument,
        StructTag,
        UnionMember,
        UnionTag,
        TypeDefinition,
        UndefinedStatic,
        EnumTag,
        EnumMember,
        RegisterParam,
        BitField,

        Block            = 0x64,
        Function,
        EndOfStruct,
        File,
        Section,
        WeakExternal,
        CLRToken,

        EndOfFunction    = 0xFF,
    }
}
