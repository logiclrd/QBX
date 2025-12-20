using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBX.CodeModel;

public class Label : IRenderableCode
{
	public string? Indentation { get; set; }
	public string Name { get; set; } = "";

	public void Render(TextWriter writer)
	{
		if (Indentation != null)
			writer.Write(Indentation);

		writer.Write(Name);
		writer.Write(':');
	}
}
