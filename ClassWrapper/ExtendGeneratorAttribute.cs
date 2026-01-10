using System;

namespace ClassWrapper;

[AttributeUsage(AttributeTargets.Class)]
public class ExtendGeneratorAttribute(string GeneratorName) : Attribute;