using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.ExecutionEngine.Marshalling;

namespace QBX.ExecutionEngine;

public class NativeProcedure(object site, MethodInfo implementation)
{
	public string Name => implementation.Name;

	// Supplied by DECLARE SUB/DECLARE FUNCTION
	public DataType[]? ParameterTypes;
	public DataType? ReturnType = null;

	public void BuildThunk()
	{
		if (ParameterTypes == null)
			throw new Exception("Internal error: BuildThunk called with no ParameterTypes");

		var parameters = implementation.GetParameters();

		if (ParameterTypes.Length != parameters.Length)
			throw RuntimeException.ArgumentCountMismatch();

		var marshallers = new Marshaller[parameters.Length];

		for (int i = 0; i < parameters.Length; i++)
		{
			var parameterType = ParameterTypes[i];

			var nativeParameterType = parameters[i].ParameterType;

			if (parameters[i].IsOut)
				nativeParameterType = nativeParameterType.GetElementType() ?? nativeParameterType;

			if (parameterType.IsPrimitiveType)
			{
				Type variableType = GetPrimitiveVariableType(parameterType);

				marshallers[i] = PrimitiveMarshaller.Construct(variableType, nativeParameterType);
			}
			else if (parameterType.IsArray)
				marshallers[i] = ArrayMarshaller.Construct(parameterType.PrimitiveType);
			else if (parameterType.IsUserType)
				marshallers[i] = UserDataTypeMarshaller.Construct(parameterType.UserType, nativeParameterType);
			else
				throw new Exception("Internal error: Don't know how to construct marshaller for type");
		}

		Type returnVariableType = ReturnType == null ? typeof(void) : GetPrimitiveVariableType(ReturnType);

		var inputParameter = Expression.Parameter(typeof(Variable[]));
		 
		int variableCount = parameters.Length;

		if (ReturnType != null)
			variableCount++;

		var marshalledArguments = new ParameterExpression[variableCount];

		for (int i = 0; i < parameters.Length; i++)
		{
			var parameterType = parameters[i].ParameterType;

			if (parameterType.IsByRef)
				parameterType = parameterType.GetElementType()!;

			marshalledArguments[i] = Expression.Variable(parameterType);
		}

		for (int i = parameters.Length; i < marshalledArguments.Length; i++)
			marshalledArguments[i] = Expression.Variable(typeof(object));

		var marshalTemporary = Expression.Variable(typeof(object));

		var marshalExpressions = new Expression[parameters.Length];

		var mapMethod = typeof(Marshaller).GetMethod(nameof(Marshaller.Map));

		if (mapMethod == null)
			throw new Exception("Sanity failure");

		var blockBody = new List<Expression>();

		for (int i = 0; i < parameters.Length; i++)
		{
			var parameterType = parameters[i].ParameterType;

			while ((parameterType != null) && parameterType.IsByRef)
				parameterType = parameterType.GetElementType();

			if (parameterType == null)
				throw new Exception("Sanity failure");

			if (!parameters[i].IsOut)
			{
				blockBody.Add(Expression.Assign(
					marshalTemporary,
					Expression.Constant(null)));

				blockBody.Add(Expression.Call(
					Expression.Constant(marshallers[i]), mapMethod,
					Expression.ArrayIndex(inputParameter, Expression.Constant(i)),
					marshalTemporary));

				blockBody.Add(Expression.Assign(
					marshalledArguments[i],
					Expression.Convert(marshalTemporary, parameterType)));
			}
		}

		Expression callExpression = Expression.Call(Expression.Constant(site), implementation, marshalledArguments);

		if (ReturnType != null)
			callExpression = Expression.Assign(marshalledArguments.Last(), callExpression);

		blockBody.Add(callExpression);
		
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].ParameterType.IsByRef
			 && !parameters[i].IsIn)
			{
				blockBody.Add(Expression.Call(
					Expression.Constant(marshallers[i]), mapMethod,
					marshalledArguments[i],
					Expression.ArrayIndex(inputParameter, Expression.Constant(i))));
			}
		}

		if (ReturnType == null)
			blockBody.Add(Expression.Constant(null, typeof(Variable)));
		else
		{
			Marshaller returnValueMarshaller;

			var returnValueVariable = marshalledArguments[0];

			if (ReturnType.IsPrimitiveType)
			{
				Type variableType = GetPrimitiveVariableType(ReturnType);

				returnValueMarshaller = PrimitiveMarshaller.Construct(implementation.ReturnType, variableType);
			}
			else if (ReturnType.IsArray)
				returnValueMarshaller = ArrayMarshaller.Construct(ReturnType.PrimitiveType);
			else if (ReturnType.IsUserType)
				returnValueMarshaller = UserDataTypeMarshaller.Construct(ReturnType.UserType, implementation.ReturnType);
			else
				throw new Exception("Internal error: Don't know how to construct marshaller for type");

			blockBody.Add(Expression.Call(
				Expression.Constant(returnValueMarshaller), mapMethod,
				marshalledArguments.Last(),
				returnValueVariable));

			blockBody.Add(Expression.Convert(
				returnValueVariable,
				typeof(Variable)));
		}

		var thunkExpression = Expression.Block(
			[.. marshalledArguments, marshalTemporary],
			blockBody);

		var thunk = Expression.Lambda<Func<Variable[], Variable>>(thunkExpression, inputParameter);

		this.Invoke = thunk.Compile();
	}

	Type GetPrimitiveVariableType(DataType type)
	{
		return
			type.PrimitiveType switch
			{
				PrimitiveDataType.Integer => typeof(IntegerVariable),
				PrimitiveDataType.Long => typeof(LongVariable),
				PrimitiveDataType.Single => typeof(SingleVariable),
				PrimitiveDataType.Double => typeof(DoubleVariable),
				PrimitiveDataType.Currency => typeof(CurrencyVariable),
				PrimitiveDataType.String => typeof(StringVariable),

				_ => throw new Exception("Internal error: Unrecognized PrimitiveDataType")
			};
	}

	public Func<Variable[], Variable>? Invoke;
}
