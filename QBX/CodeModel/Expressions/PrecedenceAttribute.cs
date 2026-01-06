using System;

namespace QBX.CodeModel.Expressions;

public class PrecedenceAttribute(int precedence) : Attribute
{
	public int Precedence => precedence;
}
