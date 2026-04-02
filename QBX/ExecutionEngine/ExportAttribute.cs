using System;

namespace QBX.ExecutionEngine;

[AttributeUsage(AttributeTargets.Method)]
class ExportAttribute : Attribute
{
	public string? Name { get; set; }
}
