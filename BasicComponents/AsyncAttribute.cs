namespace BasicComponents;

/// <summary>
/// Marque une méthode d'extension pour laquelle OtherExtension.t4 génère une surcharge
/// sur <c>Task&lt;T&gt;</c> : <c>(await self).Method(...)</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class AsyncAttribute : Attribute
{
}
