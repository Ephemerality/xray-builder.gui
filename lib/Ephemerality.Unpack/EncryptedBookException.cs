using System;

namespace Ephemerality.Unpack
{
    public sealed class EncryptedBookException : Exception
    {
        public EncryptedBookException() : base("-This book has DRM (it is encrypted). X-Ray Builder will only work on books that do not have DRM.") { }
    }
}