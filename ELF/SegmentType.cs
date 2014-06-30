
namespace DebugOS.ELF
{
    public enum HeaderType
    {
        /// <summary>
        /// Indicates that the entry is unused and the segment
        /// is to be ignored.
        /// </summary>
        Unused,
        /// <summary>
        /// The segement is loadable.
        /// </summary>
        Loadable,
        /// <summary>
        /// The segement participates in dynamic linking.
        /// </summary>
        Dynamic,
        /// <summary>
        /// The segment speciﬁes the location and size of a null-terminated path 
        /// name to invoke as an interpreter. Only one segment in an executable may
        /// be marked as such.
        /// </summary>
        InterpreterPath,
        /// <summary>
        /// The segment specifies the size and location of extended information.
        /// </summary>
        Note,
        /// <summary>
        /// The segment speciﬁes the location and size of the program header table 
        /// itself, both in the ﬁle and in the memory image of the program. 
        /// 
        /// This segment type may not occur more than once in a ﬁle. Moreover, 
        /// it may occur only if the program header table is part of the memory 
        /// image of the program. If it is present, it must precede any loadable 
        /// segment entry.
        /// </summary>
        ProgramHeaderTable,

    }
}
