using System.Collections.Generic;
using System.IO;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel;

public class CompilationUnit : IRenderableCode
{
	string _name = "Untitled";
	string _filePath = "";
	bool _hasFilePath = false;

	public string Name => _name;

	public bool HasName => _hasFilePath;

	public string FilePath
	{
		get => _filePath;
		set
		{
			_filePath = value;
			_hasFilePath = true;

			_name = Path.GetFileName(_filePath);
		}
	}

	public List<CompilationElement> Elements { get; } = new List<CompilationElement>();

	public bool IsEmpty
	{
		get
		{
			if (Elements.Count == 0)
				return true;
			if (Elements.Count > 1)
				return false;

			return (Elements[0].Lines.Count == 0);
		}
	}

	public bool IsPristine = true; // Used by DevelopmentEnvironment

	public void Render(TextWriter writer)
	{
		foreach (var element in Elements)
		{
			element.Render(writer);
			writer.WriteLine();
		}
	}

	public static CompilationUnit CreateNew()
	{
		var unit = new CompilationUnit();

		unit.Elements.Add(
			new CompilationElement(unit)
			{
				Type = CompilationElementType.Main
			});

		return unit;
	}

	// There is a specific behaviour to do with DEFtype statements preceding SUBs and FUNCTIONs.
	// Their representation differs on disk and in-memory, and QuickBASIC actually rewrites the
	// statements when saving and loading.
	//
	// On disk, there is only one DEFtype configuration, and it is updated continuously across the
	// file. DEFtype statements apply starting on the line they're encountered going forward,
	// regardless of whether that line is in the main module, preceding a SUB or FUNCTION or
	// in a SUB or FUNCTION.
	//
	// In memory, every SUB and FUNCTION starts at a baseline of DEFSNG A-Z, and specifies the
	// DEFtype statements needed to achieve the desired configuration at the start of the SUB
	// or FUNCTION directly before the opening line (and before any comments that precede the
	// SUB or FUNCTION as well).
	//
	// Thus:
	// * When loading a file, we need to translate each SUB and FUNCTION's preceding
	//   DEFtype lines from being relative to the file's state up to that point to being
	//   relative to DEFSNG A-Z, keeping track of how any DEFtype statements within the
	//   SUB or FUNCTION make changes as well.
	// * When saving a file, we need to track how the file's global DEFtype state is
	//   changing and translate the DEFtypes before each SUB and FUNCTION from being
	//   relative to DEFSNG A-Z to being relative to the current file state.
	//
	// These methods, Read and Write, perform these translations.

	public static CompilationUnit Read(TextReader reader, string filePath, BasicParser parser, bool ignoreErrors = false)
	{
		var lexer = new Lexer(reader);

		var unit = parser.Parse(lexer, ignoreErrors);

		unit.FilePath = filePath;

		var allSingleTypeMap = CompilationElement.MakeDefaultDefTypeMap();
		var identifierTypesGlobalState = CompilationElement.MakeDefaultDefTypeMap();
		var stateAfterCurrentElement = CompilationElement.MakeDefaultDefTypeMap();

		unit.Elements[0].ApplyDefTypeStatements(stateAfterCurrentElement, stopAtSubroutineOpeningStatement: false);

		for (int i = 1; i < unit.Elements.Count; i++)
		{
			stateAfterCurrentElement.CopyTo(identifierTypesGlobalState, 0);

			unit.Elements[i].ApplyDefTypeStatements(stateAfterCurrentElement, stopAtSubroutineOpeningStatement: false);
			unit.Elements[i].RewriteDefTypeStatements(
				oldRelativeTo: identifierTypesGlobalState,
				newRelativeTo: allSingleTypeMap);
		}

		return unit;
	}

	public void Write(TextWriter writer)
	{
		var allSingleTypeMap = CompilationElement.MakeDefaultDefTypeMap();
		var identifierTypesGlobalState = CompilationElement.MakeDefaultDefTypeMap();

		bool lastLineEmpty = false;

		for (int i = 0; i < Elements.Count; i++)
		{
			var element = Elements[i];

			if (i > 0)
			{
				element = element.Clone();
				element.RewriteDefTypeStatements(
					oldRelativeTo: allSingleTypeMap,
					newRelativeTo: identifierTypesGlobalState);

				if (!lastLineEmpty)
					writer.WriteLine();
			}

			foreach (var line in element.Lines)
			{
				line.Render(writer);
				lastLineEmpty = line.IsEmpty;
			}

			element.ApplyDefTypeStatements(identifierTypesGlobalState, stopAtSubroutineOpeningStatement: false);
		}
	}
}
