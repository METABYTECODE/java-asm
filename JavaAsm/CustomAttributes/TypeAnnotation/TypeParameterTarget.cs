﻿using System.IO;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class TypeParameterTarget : TypeAnnotationTarget
    {
        public byte TypeParameterIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.TypeParameter;

        internal override void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte(TypeParameterIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            TypeParameterIndex = (byte) stream.ReadByte();
        }
    }
}