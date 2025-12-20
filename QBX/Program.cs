using QBX.LexicalAnalysis;

class Program
{
	static void Main()
	{
		using (var reader = new StreamReader(@"C:\code\QBX\Samples\NIBBLES.BAS"))
		{
			var lexer = new Lexer(reader);

			var nonks = new HashSet<string>();

			nonks.Add("TRUE");
			nonks.Add("FALSE");
			nonks.Add("STARTOVER");
			nonks.Add("NEXTLEVEL");
			nonks.Add("SAMELEVEL");
			nonks.Add("MAXSNAKELENGTH");

			foreach (var token in lexer)
				if (token.Type == TokenType.Identifier)
				{
					if ((token.Value == token.Value?.ToUpper()) && !nonks.Contains(token.Value!))
						Console.WriteLine(token);
				}
		}
	}
}
