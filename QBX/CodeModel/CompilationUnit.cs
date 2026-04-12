using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using QBX.CodeModel.Statements;
using QBX.DevelopmentEnvironment;
using QBX.ExecutionEngine.Compiled;
using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Utility;

namespace QBX.CodeModel;

public class CompilationUnit : IRenderableCode, IEditableUnit
{
	string _name = "Untitled";
	string _filePath = "";
	bool _hasFilePath = false;

	public bool IsPristine { get; set; }

	public bool EnableSmartEditor => true;
	public bool IncludeInBuild { get; set; } = true;

	public IdentifierRepository IdentifierRepository { get; } = new IdentifierRepository();

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

	IReadOnlyList<IEditableElement> IEditableUnit.Elements => Elements;

	public CompilationElement MainElement => Elements.First(element => element.Type == CompilationElementType.Main);

	IEditableElement IEditableUnit.MainElement => MainElement;

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

	public CompilationUnit()
	{
		IsPristine = true;
	}

	public void Render(TextWriter writer)
	{
		foreach (var element in Elements)
		{
			element.Render(writer);
			writer.WriteLine();
		}
	}

	public void AddElement(IEditableElement element)
	{
		Elements.Add((CompilationElement)element);
	}

	public void RemoveElement(IEditableElement element)
	{
		Elements.Remove((CompilationElement)element);
	}

	public void SortElements()
	{
		Elements.Sort(
			(left, right) =>
			{
				bool leftIsMain = (left.Type == CompilationElementType.Main);
				bool rightIsMain = (right.Type == CompilationElementType.Main);

				int order = -leftIsMain.CompareTo(rightIsMain);

				if (order == 0)
					order = StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);

				return order;
			});
	}

	public void GenerateDeclarations()
	{
		// On save, QuickBASIC generates DECLARE SUB/DECLARE FUNCTION lines for every local
		// SUB/FUNCTION (except for SUBs that aren't referenced in any statement). These are
		// added only if there is no existing declaration for the same name. DECLARE FUNCTION
		// declarations include the type declaration character, but the type character is
		// ignored for the purposes of checking if the function is already declared.

		var referencedSUBs = new HashSet<Identifier>();

		foreach (var statement in Elements.SelectMany(element => element.AllStatements).OfType<CallStatement>())
			referencedSUBs.Add(statement.TargetName);

		var declaredSUBs = new HashSet<Identifier>();
		var declaredFUNCTIONs = new HashSet<Identifier>();

		foreach (var statement in MainElement.AllStatements.OfType<DeclareStatement>())
		{
			switch (statement.DeclarationType.Type)
			{
				case TokenType.SUB: declaredSUBs.Add(statement.Name); break;
				case TokenType.FUNCTION: declaredFUNCTIONs.Add(Mapper.UnqualifyIdentifier(statement.Name)); break;
			}
		}

		var newDeclarations = new List<CodeLine>();

		foreach (var element in Elements)
		{
			var displayName = element.DisplayName;

			if (displayName == null)
				continue;

			switch (element.Type)
			{
				case CompilationElementType.Sub:
					if (declaredSUBs.Contains(displayName))
						continue;
					if (!referencedSUBs.Contains(displayName))
						continue;
					break;
				case CompilationElementType.Function:
					if (declaredFUNCTIONs.Contains(displayName))
						continue;
					break;

				default: continue;
			}

			var declarationLineNumber = new MutableBox<int>();

			var declarationTokenType =
				element.Type switch
				{
					CompilationElementType.Sub => TokenType.SUB,
					CompilationElementType.Function => TokenType.FUNCTION,
					_ => throw new Exception("Sanity failure")
				};

			var declarationTypeToken = new Token(
				declarationLineNumber,
				column: 8,
				declarationTokenType,
				declarationTokenType.ToString());

			var qualifiedName = displayName;

			if (element.Type == CompilationElementType.Function)
			{
				var typeMap = CompilationElement.MakeDefaultDefTypeMap();

				element.ApplyDefTypeStatements(typeMap, stopAtSubroutineOpeningStatement: true);

				int identifierTypeIndex = char.ToUpperInvariant(displayName.Value[0]) - 'A';

				var returnType = typeMap[identifierTypeIndex];

				var returnTypeCharacter = new TypeCharacter(returnType);

				qualifiedName = new QualifiedIdentifier(qualifiedName, returnTypeCharacter);
			}

			var nameToken = new Token(
				declarationLineNumber,
				declarationTypeToken.Column + declarationTypeToken.Length + 1,
				TokenType.Identifier,
				qualifiedName.ToString());

			var openingStatement = element.AllStatements.OfType<SubroutineOpeningStatement>().FirstOrDefault();

			var declaration = new DeclareStatement(
				declarationTypeToken,
				qualifiedName,
				nameToken: null,
				openingStatement?.Parameters ?? new ParameterList());

			// Format and re-parse the statement to give it its own tokens.
			var buffer = new StringWriter();

			declaration.Render(buffer);

			var lexer = new Lexer(buffer.ToString(), MainElement);

			var parser = new BasicParser(IdentifierRepository);

			var parsedDeclaration = parser.ParseCodeLines(lexer, ignoreErrors: true).FirstOrDefault(); ;

			if (parsedDeclaration != null)
				newDeclarations.Add(parsedDeclaration);
		}

		if (newDeclarations.Any())
			MainElement.InsertLines(0, newDeclarations);
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

	public static CompilationUnit Read(TextReader reader, string filePath, int tabSize, bool ignoreErrors = false, Action<int>? lineCountCallback = null)
	{
		var lexer = new Lexer(reader);

		lexer.TabSize = tabSize;

		var unit = BasicParser.Parse(lexer, ignoreErrors, lineCountCallback);

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

	public void PrepareForWrite()
	{
		SortElements();
		GenerateDeclarations();
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
