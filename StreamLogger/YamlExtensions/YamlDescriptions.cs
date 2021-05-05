using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization.TypeInspectors;

namespace StreamLogger.YamlExtensions
{
    public class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this.innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return innerTypeDescriptor
                .GetProperties(type, container)
                .Select(d => new CommentsPropertyDescriptor(d));
        }

        private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
        {
            private readonly IPropertyDescriptor baseDescriptor;

            public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
            {
                this.baseDescriptor = baseDescriptor;
                Name = baseDescriptor.Name;
            }

            public string Name { get; set; }

            public Type Type { get { return baseDescriptor.Type; } }

            public Type TypeOverride
            {
                get { return baseDescriptor.TypeOverride; }
                set { baseDescriptor.TypeOverride = value; }
            }

            public int Order { get; set; }

            public ScalarStyle ScalarStyle
            {
                get { return baseDescriptor.ScalarStyle; }
                set { baseDescriptor.ScalarStyle = value; }
            }

            public bool CanWrite { get { return baseDescriptor.CanWrite; } }

            public void Write(object target, object value)
            {
                baseDescriptor.Write(target, value);
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return baseDescriptor.GetCustomAttribute<T>();
            }

            public IObjectDescriptor Read(object target)
            {
                var description = baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
                return description != null
                    ? new CommentsObjectDescriptor(baseDescriptor.Read(target), description.Description)
                    : baseDescriptor.Read(target);
            }
        }
    }

    public sealed class CommentsObjectDescriptor : IObjectDescriptor
    {
        private readonly IObjectDescriptor innerDescriptor;

        public CommentsObjectDescriptor(IObjectDescriptor innerDescriptor, string comment)
        {
            this.innerDescriptor = innerDescriptor;
            this.Comment = comment;
        }

        public string Comment { get; private set; }

        public object Value { get { return innerDescriptor.Value; } }
        public Type Type { get { return innerDescriptor.Type; } }
        public Type StaticType { get { return innerDescriptor.StaticType; } }
        public ScalarStyle ScalarStyle { get { return innerDescriptor.ScalarStyle; } }
    }

    public class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public CommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            if (value is CommentsObjectDescriptor commentsDescriptor && commentsDescriptor.Comment != null)
            {
                context.Emit(new Comment(commentsDescriptor.Comment, false));
            }

            return base.EnterMapping(key, value, context);
        }
    }
	
    public sealed class ValidatingNodeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer nodeDeserializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatingNodeDeserializer"/> class.
        /// </summary>
        /// <param name="nodeDeserializer">The node deserializer instance.</param>
        public ValidatingNodeDeserializer(INodeDeserializer nodeDeserializer) => this.nodeDeserializer = nodeDeserializer;

        /// <inheritdoc/>
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value))
            {
                Validator.ValidateObject(value, new ValidationContext(value, null, null), true);

                return true;
            }

            return false;
        }
    }
}