﻿using System;
using System.Collections.Generic;
using System.Linq;
using JavaDeobfuscator.JavaAsm.CustomAttributes;
using JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation;
using JavaDeobfuscator.JavaAsm.Instructions;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm
{
    internal class MethodNode
    {
        public ClassNode Owner { get; set; }


        public MethodAccessModifiers Access { get; set; }

        public string Name { get; set; }

        public MethodDescriptor Descriptor { get; set; }

        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();


        public string Signature { get; set; }

        public ushort MaxStack { get; set; }

        public ushort MaxLocals { get; set; }

        public List<TryCatchNode> TryCatches { get; set; } = new List<TryCatchNode>();

        public InstructionList Instructions { get; set; } = new InstructionList();

        public List<AttributeNode> CodeAttributes { get; set; } = new List<AttributeNode>();

        public List<AnnotationNode> InvisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public List<AnnotationNode> VisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public bool IsDeprecated { get; set; }

        public List<ClassName> Throws { get; set; } = new List<ClassName>();

        public ElementValue AnnotationDefaultValue { get; set; }

        private AttributeNode GetAttribute(string name)
        {
            var attribute = Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                Attributes.Remove(attribute);
            return attribute;
        }

        internal void Parse(ClassReaderState readerState)
        {
            Signature = (GetAttribute(PredefinedAttributeNames.Signature)?.ParsedAttribute as SignatureAttribute)?.Value;
            {
                var attribute = GetAttribute(PredefinedAttributeNames.Code);
                if (attribute?.ParsedAttribute is CodeAttribute codeAttribute)
                    InstructionListConverter.ParseCodeAttribute(this, readerState, codeAttribute);
            }
            {
                var attribute = GetAttribute(PredefinedAttributeNames.RuntimeInvisibleAnnotations);
                if (attribute != null)
                    InvisibleAnnotations = (attribute.ParsedAttribute as RuntimeInvisibleAnnotationsAttribute)?.Annotations;
            }
            {
                var attribute = GetAttribute(PredefinedAttributeNames.RuntimeVisibleAnnotations);
                if (attribute != null)
                    VisibleAnnotations = (attribute.ParsedAttribute as RuntimeVisibleAnnotationsAttribute)?.Annotations;
            }
            {
                var attribute = GetAttribute(PredefinedAttributeNames.Exceptions);
                if (attribute != null)
                    Throws = (attribute.ParsedAttribute as ExceptionsAttribute)?.ExceptionTable;
            }
            AnnotationDefaultValue =
                (GetAttribute(PredefinedAttributeNames.AnnotationDefault)?.ParsedAttribute as AnnotationDefaultAttribute)?.Value;
            IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
        }

        internal void Save(ClassWriterState writerState)
        {
            if (Signature != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.Signature))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Signature} attribute is already presented on method");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Signature,
                    ParsedAttribute = new SignatureAttribute
                    {
                        Value = Signature
                    }
                });
            }

            if (!Access.HasFlag(MethodAccessModifiers.Abstract) && !Access.HasFlag(MethodAccessModifiers.Native) && Instructions != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.Code))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Code} attribute is already presented on method");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Code,
                    ParsedAttribute = InstructionListConverter.SaveCodeAttribute(this, writerState)
                });
            }

            if (InvisibleAnnotations.Count > 0)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeInvisibleAnnotations))
                    throw new Exception(
                        $"{PredefinedAttributeNames.RuntimeInvisibleAnnotations} attribute is already presented on method");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.RuntimeInvisibleAnnotations,
                    ParsedAttribute = new RuntimeInvisibleAnnotationsAttribute
                    {
                        Annotations = InvisibleAnnotations
                    }
                });
            }

            if (VisibleAnnotations.Count > 0)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeVisibleAnnotations))
                    throw new Exception(
                        $"{PredefinedAttributeNames.RuntimeVisibleAnnotations} attribute is already presented on method");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.RuntimeVisibleAnnotations,
                    ParsedAttribute = new RuntimeVisibleAnnotationsAttribute
                    {
                        Annotations = VisibleAnnotations
                    }
                });
            }

            if (Throws.Count > 0)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.Exceptions))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Exceptions} attribute is already presented on method");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Exceptions,
                    ParsedAttribute = new ExceptionsAttribute
                    {
                        ExceptionTable = Throws
                    }
                });
            }

            if (AnnotationDefaultValue != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.AnnotationDefault))
                    throw new Exception(
                        $"{PredefinedAttributeNames.AnnotationDefault} attribute is already presented on method");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.AnnotationDefault,
                    ParsedAttribute = new AnnotationDefaultAttribute
                    {
                        Value = AnnotationDefaultValue
                    }
                });
            }

            // ReSharper disable once InvertIf
            if (IsDeprecated)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.Deprecated))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Deprecated} attribute is already presented on method");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Deprecated,
                    ParsedAttribute = new DeprecatedAttribute()
                });
            }
        }

        public override string ToString()
        {
            return $"{AccessModifiersExtensions.ToString(Access)} {Name}{Descriptor}";
        }
    }
}