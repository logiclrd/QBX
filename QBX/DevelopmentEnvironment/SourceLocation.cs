using QBX.CodeModel;
using QBX.CodeModel.Statements;

namespace QBX.DevelopmentEnvironment;

public class SourceLocation(CompilationUnit unit, CompilationElement element, CodeLine line, Statement statement, int lineIndex)
{
	public CompilationUnit Unit => unit;
	public CompilationElement Element => element;
	public CodeLine Line => line;
	public Statement Statement => statement;
	public int LineIndex => lineIndex;
}
