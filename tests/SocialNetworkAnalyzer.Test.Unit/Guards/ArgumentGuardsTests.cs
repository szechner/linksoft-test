using FluentAssertions;
using SocialNetworkAnalyzer.Core.Guards;

namespace SocialNetworkAnalyzer.Test.Unit.Guards;

public class ArgumentGuardsTests
{
    [OneTimeSetUp]
    public void Setup()
    {
    }

    [Test]
    public void Throw_Exception_Without_Message()
    {
        object? testObject = null;
        
        Action execute = () => Guard.Require.ArgumentNotNull(testObject);
        execute.Should().Throw<ArgumentNullException>().WithParameterName(nameof(Throw_Exception_Without_Message)).WithMessage($"Value cannot be null. (Parameter '{nameof(Throw_Exception_Without_Message)}')");
    }
    
    [Test]
    public void Throw_Exception_With_Message()
    {
        object? testObject = null;
        
        Action execute = () => Guard.Require.ArgumentNotNull(testObject, message:"test");
        execute.Should().Throw<ArgumentNullException>().WithParameterName(nameof(Throw_Exception_With_Message)).WithMessage($"test (Parameter '{nameof(Throw_Exception_With_Message)}')");
    }
    
    [Test]
    public void Throw_Exception_With_ParameterName()
    {
        object? testObject = null;
        
        Action execute = () => Guard.Require.ArgumentNotNull(testObject, nameof(testObject));
        execute.Should().Throw<ArgumentNullException>().WithParameterName(nameof(testObject)).WithMessage("Value cannot be null. (Parameter 'testObject')");
    }
    
    [Test]
    public void Throw_Exception_With_ParameterName_And_Message()
    {
        object? testObject = null;
        
        Action execute = () => Guard.Require.ArgumentNotNull(testObject, nameof(testObject), "test");
        execute.Should().Throw<ArgumentNullException>().WithParameterName(nameof(testObject)).WithMessage("test (Parameter 'testObject')");
    }

    [Test]
    public void Validate_That_The_Object_Is_Not_Null()
    {
        object? testObject = null;
        testObject = new object();
        
        Action execute = () => Guard.Require.ArgumentNotNull(testObject, nameof(testObject), "test");
        execute.Should().NotThrow<ArgumentNullException>();
    }
}