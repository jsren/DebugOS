using System.Collections.Generic;
using System.Linq;

namespace DebugOS
{
    public class LiveCodeUnit : CodeUnit, IDebugResource
    {
        public List<CodeLine> lines;

        public LiveCodeUnit() {
            this.lines = new List<CodeLine>();
        }

        public override CodeLine[] Lines {
            get { return this.lines.ToArray(); }
        }

        public override long Size {
            get { return this.lines.Sum(l => l.Size); }
        }

        public override long Offset {
            get { return this.lines.Any() ? this.lines[0].Offset : -1; }
        }

        public override string Name {
            get { return "<untitled>"; }
        }

        public void AddCodeLine(CodeLine line) {
            this.lines.Add(line);
        }

        public void AddAssemblyLine(AssemblyLine line)
        {
            this.lines.Add(new CodeLine(line.MachineCode.Length, 
                line.Offset, -1, "", new AssemblyLine[] { line }));
        }
    }
}
