namespace ClassWrapper;
using ClassWrapperGenerator.Extendables;

[AttributeUsage(AttributeTargets.Class)]
public class ExtendAttribute(params string[] additionalFilesPattern) : Attribute;


[AttributeUsage(AttributeTargets.Method)]
[ExtendGenerator(nameof(AsyncExtend))]
public class AsyncAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
[ExtendGenerator(nameof(ToStringExtend))]
public class ToStringAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
[ExtendGenerator(nameof(MockFromJsonExtend))]
public class MockFromJsonAttribute(Type interfaceToImplement) : Attribute;

[AttributeUsage(AttributeTargets.Method)]
[ExtendGenerator(nameof(SetFieldExtend))]
public class SetFieldAttribute(string? fieldName = null, string? returnValue = null) : Attribute;

[AttributeUsage(AttributeTargets.Method)]
[ExtendGenerator(nameof(GetFieldExtend))]
public class GetFieldAttribute(string? fieldName = null) : Attribute;

[AttributeUsage(AttributeTargets.Method)]
[ExtendGenerator(nameof(LoadJsonFileExtend))]
public class LoadJsonFile(params string[] ignoredField) : Attribute;
