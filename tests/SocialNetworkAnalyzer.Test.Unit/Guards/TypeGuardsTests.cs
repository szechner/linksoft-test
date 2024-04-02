using FluentAssertions;
using SocialNetworkAnalyzer.Core.Guards;

namespace SocialNetworkAnalyzer.Test.Unit.Guards;

public class TypeGuardsTests
{
    [OneTimeSetUp]
    public void Setup()
    {
    }

    [Test]
    public void Throw_Exception_Without_Message()
    {
        var testObject = new TypeGuardsTests();
        
        Action execute = () => Guard.Require.TypeOf<int>(testObject);
        execute.Should().Throw<InvalidCastException>().WithMessage($"Cannot cast {nameof(TypeGuardsTests)} to {nameof(Int32)}. {nameof(Throw_Exception_Without_Message)} is not of type {nameof(Int32)}.");
    }
    
    [Test]
    public void Throw_Exception_With_Message()
    {
        object? testObject = null;
        
        Action execute = () => Guard.Require.TypeOf<int>(testObject, message:"test");
        execute.Should().Throw<InvalidCastException>().WithMessage("test");
    }
    
    [Test]
    public void Throw_Exception_With_ParameterName()
    {
        object? testObject = null;
        
        Action execute = () => Guard.Require.TypeOf<int>(testObject, nameof(testObject));
        execute.Should().Throw<InvalidCastException>().WithMessage($"Cannot cast input to {nameof(Int32)}. {nameof(testObject)} is not of type {nameof(Int32)}.");
    }
    
    [Test]
    public void Throw_Exception_With_ParameterName_And_Message()
    {
        object? testObject = null;
        
        Action execute = () => Guard.Require.TypeOf<int>(testObject, nameof(testObject), "test");
        execute.Should().Throw<InvalidCastException>().WithMessage("test");
    }

    [Test]
    public void Validate_Int_Type()
    {
        var testObject = 10;
        
        Action execute = () => Guard.Require.TypeOf<int>(testObject, nameof(testObject), "test");
        execute.Should().NotThrow<InvalidCastException>();
    }
}