using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class TypeRepository
{
	Dictionary<UserDataType, UserDataType> _userDataTypes = new Dictionary<UserDataType, UserDataType>();

	public UserDataType RegisterType(UserDataType userType)
	{
		if (_userDataTypes.TryGetValue(userType, out var previousDefinition))
			return previousDefinition;

		_userDataTypes[userType] = userType;

		return userType;
	}
}
