﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using EasyNetQ.Tests.ProducerTests.Very.Long.Namespace.Certainly.Longer.Than.The255.Char.Length.That.RabbitMQ.Likes.That.Will.Certainly.Cause.An.AMQP.Exception.If.We.Dont.Do.Something.About.It.And.Stop.It.From.Happening;
using NUnit.Framework;

namespace EasyNetQ.Tests
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class DynamicTypeNameSerializerTests
    {
        private const string expectedTypeName = "System.String:mscorlib";
        private const string expectedCustomTypeName = "EasyNetQ.Tests.DynamicTypeNameSerializer_SomeRandomClass:EasyNetQ.Tests";

        private ITypeNameSerializer typeNameSerializer;

        [SetUp]
        public void SetUp()
        {
            typeNameSerializer = new DynamicTypeNameSerializer();
        }

        [Test]
        public void Should_serialize_a_type_name()
        {
            var typeName = typeNameSerializer.Serialize(typeof(string));
            typeName.ShouldEqual(expectedTypeName);
        }

        [Test]
        public void Should_serialize_a_custom_type()
        {
            var typeName = typeNameSerializer.Serialize(typeof(DynamicTypeNameSerializer_SomeRandomClass));
            typeName.ShouldEqual(expectedCustomTypeName);
        }

        [Test]
        public void Should_serialize_a_dynamic_type()
        {
            var dynamicAssembyName = Guid.NewGuid().ToString("N");
            var dynamicTypeName = Guid.NewGuid().ToString("N");
            var dynamicType = CreateDynamicType(dynamicAssembyName, dynamicTypeName);
            var expectedDynamicTypeName = $"{dynamicTypeName}:{dynamicAssembyName}";

            var typeName = typeNameSerializer.Serialize(dynamicType);
            typeName.ShouldEqual(expectedDynamicTypeName);
        }

        [Test]
        public void Should_deserialize_a_type_name()
        {
            var type = typeNameSerializer.DeSerialize(expectedTypeName);
            type.ShouldEqual(typeof(string));
        }

        [Test]
        public void Should_deserialize_a_custom_type()
        {
            var type = typeNameSerializer.DeSerialize(expectedCustomTypeName);
            type.ShouldEqual(typeof(DynamicTypeNameSerializer_SomeRandomClass));
        }

        [Test]
        public void Should_deserialize_a_dynamic_type()
        {
            var dynamicAssembyName = Guid.NewGuid().ToString("N");
            var dynamicTypeName = Guid.NewGuid().ToString("N");
            var dynamicType = CreateDynamicType(dynamicAssembyName, dynamicTypeName);
            var serializedDynamicTypeName = $"{dynamicTypeName}:{dynamicAssembyName}";

            var type = typeNameSerializer.DeSerialize(serializedDynamicTypeName);
            type.ShouldEqual(dynamicType);
        }

        [Test]
        [ExpectedException(typeof(EasyNetQException))]
        public void Should_throw_exception_when_type_name_is_not_recognised()
        {
            typeNameSerializer.DeSerialize("EasyNetQ.DynamicTypeNameSerializer.None:EasyNetQ");
        }

        [Test]
        [ExpectedException(typeof(EasyNetQException))]
        public void Should_throw_if_type_name_is_too_long()
        {
            typeNameSerializer.Serialize(
                typeof(
                    MessageWithVeryVEryVEryLongNameThatWillMostCertainlyBreakAmqpsSilly255CharacterNameLimitThatIsAlmostCertainToBeReachedWithGenericTypes
                ));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_throw_exception_if_type_name_is_null()
        {
            typeNameSerializer.DeSerialize(null);
        }

        public void Spike()
        {
            var type = Type.GetType("EasyNetQ.Tests.DynamicTypeNameSerializer_SomeRandomClass, EasyNetQ.Tests");
            type.ShouldEqual(typeof(DynamicTypeNameSerializer_SomeRandomClass));
        }

        public void Spike2()
        {
            var name = typeof(DynamicTypeNameSerializer_SomeRandomClass).AssemblyQualifiedName;
            Console.Out.WriteLine(name);
        }

        private static Type CreateDynamicType(string assemblyName, string typeName)
        {
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule(assemblyName + "_module");
            var typeBuilder = module.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
            return typeBuilder.CreateType();
        }
    }

    public class DynamicTypeNameSerializer_SomeRandomClass
    {

    }
    // ReSharper restore InconsistentNaming
}