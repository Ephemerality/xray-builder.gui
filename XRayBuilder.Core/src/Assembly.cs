using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("XRayBuilder.Test")]

// TODO Remove once this bug in C# 9 is fixed
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit{}
} 