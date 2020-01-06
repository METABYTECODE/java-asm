﻿using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes
{
    public class ParameterAnnotations
    {
        public List<AnnotationNode> Annotations { get; set; } = new List<AnnotationNode>();
    }

    public class RuntimeInvisibleParameterAnnotationsAttribute : CustomAttribute
    {

        public List<ParameterAnnotations> Parameters { get; set; } = new List<ParameterAnnotations>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Parameters.Count > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"Number of parameters is too big: {Parameters.Count} > {byte.MaxValue}");
            attributeDataStream.WriteByte((byte)Parameters.Count);
            foreach (var parameter in Parameters)
            {
                if (parameter.Annotations.Count > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(
                        $"Number of annotations is too big: {parameter.Annotations.Count} > {ushort.MaxValue}");
                Binary.BigEndian.Write(attributeDataStream, (ushort)parameter.Annotations.Count);
                foreach (var annotation in parameter.Annotations)
                    annotation.Write(attributeDataStream, writerState);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeInvisibleParameterAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeInvisibleParameterAnnotationsAttribute>
    {
        public RuntimeInvisibleParameterAnnotationsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new RuntimeInvisibleParameterAnnotationsAttribute();

            var parametersCount = (byte)attributeDataStream.ReadByte();
            attribute.Parameters.Capacity = parametersCount;
            for (var i = 0; i < parametersCount; i++)
            {
                var parameter = new ParameterAnnotations();
                var annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
                parameter.Annotations.Capacity = annotationsCount;
                for (var j = 0; j < annotationsCount; j++)
                    parameter.Annotations.Add(AnnotationNode.Parse(attributeDataStream, readerState));
                attribute.Parameters.Add(parameter);
            }

            return attribute;
        }
    }
}