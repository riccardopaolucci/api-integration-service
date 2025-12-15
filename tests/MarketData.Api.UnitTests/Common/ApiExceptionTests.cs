using MarketData.Api.Common.Errors;
using Xunit;

namespace MarketData.Api.UnitTests.Common;

public class ApiExceptionTests
{
    [Fact]
    public void Constructor_Sets_Code_Status_Message_And_Details()
    {
        // Arrange
        var ex = new ApiException(
            errorCode: ErrorCodes.Unauthorized,
            statusCode: 401,
            message: "Invalid credentials.",
            details: "Password mismatch.");

        // Assert
        Assert.Equal(ErrorCodes.Unauthorized, ex.ErrorCode);
        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("Invalid credentials.", ex.Message);
        Assert.Equal("Password mismatch.", ex.Details);
    }

    [Fact]
    public void Unauthorized_Factory_Creates_401_With_Code_Unauthorized()
    {
        var ex = ApiException.Unauthorized("Nope.");

        Assert.Equal(ErrorCodes.Unauthorized, ex.ErrorCode);
        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("Nope.", ex.Message);
    }

    [Fact]
    public void Validation_Factory_Creates_400_With_Code_ValidationError()
    {
        var ex = ApiException.Validation("Bad input.");

        Assert.Equal(ErrorCodes.ValidationError, ex.ErrorCode);
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Bad input.", ex.Message);
    }

    [Fact]
    public void NotFound_Factory_Creates_404_With_Code_NotFound()
    {
        var ex = ApiException.NotFound("Missing.");

        Assert.Equal(ErrorCodes.NotFound, ex.ErrorCode);
        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("Missing.", ex.Message);
    }

    [Fact]
    public void ExternalFailure_Factory_Creates_502_With_Code_ExternalServiceFailure()
    {
        var ex = ApiException.ExternalFailure("Upstream down.");

        Assert.Equal(ErrorCodes.ExternalServiceFailure, ex.ErrorCode);
        Assert.Equal(502, ex.StatusCode);
        Assert.Equal("Upstream down.", ex.Message);
    }
}
