﻿using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using StructureMap.Attributes;
using StructureMap.Building;
using StructureMap.Pipeline;
using StructureMap.Testing.Graph;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget2;
using StructureMap.Testing.Widget3;
using StructureMap.TypeRules;

namespace StructureMap.Testing.Building
{
    [TestFixture]
    public class ConcreteTypeTester
    {
        [Test]
        public void no_value_for_non_simple_resolves_to_default_source()
        {
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (IGateway), null)
                .ShouldBeOfType<DefaultDependencySource>()
                .DependencyType.ShouldEqual(typeof (IGateway));
        }

        [Test]
        public void value_is_instance_for_non_simple_resolves_to_lifecycle_source()
        {
            var instance = new FakeInstance();
            instance.SetLifecycleTo(new SingletonLifecycle());

            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (IGateway), instance)
                .ShouldBeTheSameAs(instance.DependencySource);
        }

        [Test]
        public void if_value_exists_and_it_is_the_right_type_return_constant()
        {
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (string), "foo")
                .ShouldEqual(Constant.For("foo"));

            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (int), 42)
                .ShouldEqual(Constant.For<int>(42));

            // My dad raises registered Beefmasters and he'd be disappointed
            // if the default here was anything else
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (BreedEnum), BreedEnum.Beefmaster)
                .ShouldEqual(Constant.For(BreedEnum.Beefmaster));

            var gateway = new StubbedGateway();
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (IGateway), gateway)
                .ShouldEqual(Constant.For<IGateway>(gateway));
        }

        [Test]
        public void if_list_value_exists_use_that()
        {
            var list = new List<IGateway> {new StubbedGateway(), new StubbedGateway()};
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (List<IGateway>), list)
                .ShouldEqual(Constant.For(list));

            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (IList<IGateway>), list)
                .ShouldEqual(Constant.For<IList<IGateway>>(list));
        }

        [Test]
        public void coerce_simple_numbers()
        {
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (int), "42")
                .ShouldEqual(Constant.For(42));
        }

        [Test]
        public void coerce_enum()
        {
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (BreedEnum), "Angus")
                .ShouldEqual(Constant.For(BreedEnum.Angus));
        }

        [Test]
        public void array_can_be_coerced_to_concrete_list()
        {
            var array = new IGateway[] {new StubbedGateway(), new StubbedGateway()};
            var constant = ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (List<IGateway>), array)
                .ShouldBeOfType<Constant>();

            constant.ReturnedType.ShouldEqual(typeof (List<IGateway>));
            constant.Value.As<List<IGateway>>()
                .ShouldHaveTheSameElementsAs(array);
        }

        [Test]
        public void array_can_be_coerced_to_concrete_ilist()
        {
            var array = new IGateway[] {new StubbedGateway(), new StubbedGateway()};
            var constant = ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (IList<IGateway>), array)
                .ShouldBeOfType<Constant>();

            constant.ReturnedType.ShouldEqual(typeof (IList<IGateway>));
            constant.Value.As<IList<IGateway>>()
                .ShouldHaveTheSameElementsAs(array);
        }

        [Test]
        public void array_can_be_coerced_to_enumerable()
        {
            var list = new IGateway[] {new StubbedGateway(), new StubbedGateway()};
            var constant = ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (List<IGateway>), list)
                .ShouldBeOfType<Constant>();

            constant.ReturnedType.ShouldEqual(typeof (List<IGateway>));
            constant.Value.As<List<IGateway>>()
                .ShouldHaveTheSameElementsAs(list);
        }

        [Test]
        public void list_can_be_coerced_to_array()
        {
            var list = new List<IGateway> {new StubbedGateway(), new StubbedGateway()};
            var constant = ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof(IGateway[]), list)
                .ShouldBeOfType<Constant>();

            constant.ReturnedType.ShouldEqual(typeof (IGateway[]));
            constant.Value.As<IGateway[]>()
                .ShouldHaveTheSameElementsAs(list.ToArray());
        }

        [Test]
        public void use_all_possible_for_array()
        {
            var enumerableType = typeof (IGateway[]);
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", enumerableType, null)
                .ShouldEqual(new AllPossibleValuesDependencySource(enumerableType));
        }

        [Test]
        public void use_all_possible_for_ienumerable()
        {
            var enumerableType = typeof (IEnumerable<IGateway>);
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", enumerableType, null)
                .ShouldEqual(new AllPossibleValuesDependencySource(enumerableType));
        }

        [Test]
        public void use_all_possible_for_ilist()
        {
            var enumerableType = typeof (IList<IGateway>);
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", enumerableType, null)
                .ShouldEqual(new AllPossibleValuesDependencySource(enumerableType));
        }

        [Test]
        public void use_all_possible_for_list()
        {
            var enumerableType = typeof (List<IGateway>);
            ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", enumerableType, null)
                .ShouldEqual(new AllPossibleValuesDependencySource(enumerableType));
        }

        [Test]
        public void source_for_missing_string_constructor_arg()
        {
            var source = ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeProp", typeof (string), null)
                .ShouldBeOfType<DependencyProblem>();

            source.Name.ShouldEqual("SomeProp");
            source.Type.ShouldEqual(ConcreteType.ConstructorArgument);
            source.Message.ShouldEqual("Required primitive dependency is not explicitly defined");
            source.ReturnedType.ShouldEqual(typeof (string));
        }

        [Test]
        public void source_for_missing_string_setter_arg()
        {
            var source = ConcreteType.SourceFor(ConcreteType.SetterProperty, "SomeProp", typeof(string), null)
                .ShouldBeOfType<DependencyProblem>();

            source.Name.ShouldEqual("SomeProp");
            source.Type.ShouldEqual(ConcreteType.SetterProperty);
            source.Message.ShouldEqual("Required primitive dependency is not explicitly defined");
            source.ReturnedType.ShouldEqual(typeof(string));
        }

        [Test]
        public void unable_to_determine_a_dependency()
        {
            var colorRule = new ColorRule("Red");
            var source = ConcreteType.SourceFor(
                ConcreteType.SetterProperty, 
                "SomeProp", 
                typeof (IGateway),
                colorRule)
                .ShouldBeOfType<DependencyProblem>();

            source.Name.ShouldEqual("SomeProp");
            source.Type.ShouldEqual(ConcreteType.SetterProperty);
            source.ReturnedType.ShouldEqual(typeof (IGateway));
            source.Message.ShouldEqual(ConcreteType.UnableToDetermineDependency.ToFormat(
                typeof (IGateway).GetFullName(), colorRule));
        }

        [Test]
        public void source_for_conversion_problem()
        {
            var source = ConcreteType.SourceFor(ConcreteType.ConstructorArgument, "SomeArg", typeof (int), "foo")
                .ShouldBeOfType<DependencyProblem>();

            source.Name.ShouldEqual("SomeArg");
            source.Type.ShouldEqual(ConcreteType.ConstructorArgument);
            source.ReturnedType.ShouldEqual(typeof (int));
            source.Message.ShouldEqual(ConcreteType.CastingError.ToFormat("foo", typeof (string).GetFullName(),
                typeof (int).GetFullName()));
        }



        [Test]
        public void throw_a_description_exception_when_no_suitable_ctor_can_be_found()
        {
            var ex = Exception<StructureMapConfigurationException>.ShouldBeThrownBy(() => {
                ConcreteType.BuildConstructorStep(typeof (GuyWithNoSuitableCtor), null, new DependencyCollection(),
                    new Policies());
            });

            ex.Message.ShouldContain("No public constructor could be selected for concrete type " + typeof(GuyWithNoSuitableCtor).GetFullName());
        }

        [Test]
        public void is_valid_happy_path_with_ctor_checks()
        {
            // This class needs all three of these things
            var dependencies = new DependencyCollection();
            dependencies.Add("name", "Jeremy");
            dependencies.Add("age", 40);
            dependencies.Add("isAwake", true);

            ConcreteType.BuildSource(typeof(GuyWithPrimitives), null, dependencies, new Policies())
                .IsValid().ShouldBeTrue();
        }

        [Test]
        public void is_valid_sad_path_with_ctor_checks()
        {
            // This class needs all three of these things
            var dependencies = new DependencyCollection();
            dependencies.Add("name", "Jeremy");
            dependencies.Add("age", 40);
            //dependencies.Add("isAwake", true);

            ConcreteType.BuildSource(typeof(GuyWithPrimitives), null, dependencies, new Policies())
                .IsValid().ShouldBeFalse();
        }



        [Test]
        public void is_valid_happy_path_with_setter_checks()
        {
            // This class needs all three of these things
            var dependencies = new DependencyCollection();
            dependencies.Add("Name", "Jeremy");
            dependencies.Add("Age", 40);
            dependencies.Add("IsAwake", true);

            ConcreteType.BuildSource(typeof(GuyWithPrimitiveSetters), null, dependencies, new Policies())
                .IsValid().ShouldBeTrue();
        }

        [Test]
        public void is_valid_sad_path_with_setter_checks()
        {
            // This class needs all three of these things
            var dependencies = new DependencyCollection();
            dependencies.Add("Name", "Jeremy");
            dependencies.Add("Age", 40);
            //dependencies.Add("IsAwake", true);

            ConcreteType.BuildSource(typeof(GuyWithPrimitiveSetters), null, dependencies, new Policies())
                .IsValid().ShouldBeFalse();
        }




    }

    public class GuyWithNoSuitableCtor
    {
        private GuyWithNoSuitableCtor()
        {
        }
    }



    public class GuyWithPrimitives
    {
        private readonly string _name;
        private readonly int _age;
        private readonly bool _isAwake;

        public GuyWithPrimitives(string name, int age, bool isAwake)
        {
            _name = name;
            _age = age;
            _isAwake = isAwake;
        }
    }

    public class GuyWithPrimitiveEverything
    {
        private readonly string _name;
        private readonly int _age;
        private readonly bool _isAwake;

        public GuyWithPrimitiveEverything(string name, int age, bool isAwake)
        {
            _name = name;
            _age = age;
            _isAwake = isAwake;
        }

        [SetterProperty]
        public string Direction { get; set; }
    
        [SetterProperty]
        public string Description { get; set; }
    }

    public class GuyWithPrimitiveSetters
    {
        [SetterProperty]
        public string Name { get; set; }

        [SetterProperty]
        public int Age { get; set; }

        [SetterProperty]
        public bool IsAwake { get; set; }
    }
}