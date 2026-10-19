using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmartCartPro.Exceptions;
using SmartCartPro.Models;

namespace SmartCartPro.Helpers;

/// <summary>
/// Shared helpers for Catalog (and later) AJAX create/update actions:
/// validate → throw on failure → <see cref="SuccessResponse"/> on success.
/// </summary>
public static class CommonHelpers
{
    public static void ValidateModelState(ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
        {
            return;
        }

        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? e.Exception?.Message ?? "Invalid value."
                : e.ErrorMessage)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct()
            .ToList();

        throw new ValidationException(
            $"Invalid input: {string.Join(", ", errors)}",
            errors.FirstOrDefault() ?? "Invalid input.");
    }

    public static IActionResult SuccessResponse(string message, object? data = null) =>
        new OkObjectResult(CommonResponseModel.Success(message, data));
}
