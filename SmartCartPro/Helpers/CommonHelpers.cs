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

        var fieldErrors = modelState
            .Where(kv => kv.Value is { Errors.Count: > 0 })
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? e.Exception?.Message ?? "Invalid value."
                        : e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct()
                    .ToArray());

        var errors = fieldErrors.Values.SelectMany(v => v).Distinct().ToList();
        var summary = errors.Count > 0
            ? string.Join(" ", errors)
            : "Invalid input.";

        throw new ValidationException(summary, summary, fieldErrors);
    }

    public static IActionResult SuccessResponse(string message, object? data = null) =>
        new OkObjectResult(CommonResponseModel.Success(message, data));
}
