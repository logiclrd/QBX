using System;
using System.Collections.Generic;
using System.Linq;

namespace QBX.ExecutionEngine.Compiled;

public class UserDataType : IEquatable<UserDataType>
{
	public Dictionary<Module, UserDataTypeFacade> Facades { get; } = new();

	public List<UserDataTypeField> Fields { get; } = new();

	public int CalculateByteSize()
		=> Fields.Sum(field => field.Type.ByteSize);

	public override bool Equals(object? obj)
		=> Equals(obj as UserDataType);

	public bool Equals(UserDataType? other)
	{
		if (other == null)
			return false;

		if (Fields.Count != other.Fields.Count)
			return false;

		for (int i = 0; i < Fields.Count; i++)
		{
			var thisType = Fields[i].Type;
			var otherType = other.Fields[i].Type;

			if (thisType.Equals(otherType))
				continue;

			if (thisType.IsUserType != otherType.IsUserType)
				return false;

			if (!thisType.IsUserType
			 || !thisType.UserType.Equals(otherType.UserType))
				return false;

			var thisSubscripts = Fields[i].ArraySubscripts;
			var otherSubscripts = Fields[i].ArraySubscripts;

			if (thisSubscripts == null)
			{
				if ((otherSubscripts != null) && (otherSubscripts.Subscripts.Count > 0))
					return false;
			}
			else
			{
				if (!thisSubscripts.Equals(otherSubscripts))
					return false;
			}
		}

		return true;
	}

	public override int GetHashCode()
	{
		int hashCode = Fields.Count;

		for (int i = 0; i < Fields.Count; i++)
		{
			hashCode = unchecked((hashCode * 1061) ^ (hashCode >> 24));
			hashCode ^= Fields[i].GetHashCode();
		}

		return hashCode;
	}
}
