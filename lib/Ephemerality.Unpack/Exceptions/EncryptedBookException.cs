using System;

namespace Ephemerality.Unpack.Exceptions
{
    public sealed class EncryptedBookException : Exception
    {
        public EncryptedBookException() : base("This book has DRM (it is encrypted) and could not be opened.") { }
    }
}