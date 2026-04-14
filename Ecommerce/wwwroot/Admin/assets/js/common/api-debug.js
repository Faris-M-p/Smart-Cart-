// Common API failure logger.
// Logs ONLY on failures (non-OK responses, business failures, or caught exceptions).
(function () {
  function safeClone(value) {
    try {
      return JSON.parse(JSON.stringify(value));
    } catch {
      return value;
    }
  }

  function summarizeResult(result) {
    if (result == null) return null;
    const msg =
      result.responseMsg ??
      result.ResponseMsg ??
      result.message ??
      result.Message ??
      null;
    const errs = result.errors ?? result.Errors ?? null;
    const sc = result.statusCode ?? result.StatusCode ?? null;
    const ok = result.success ?? result.Success ?? null;
    return { message: msg, errors: errs, statusCode: sc, success: ok };
  }

  function logFailure(context, details) {
      try {
        const d = details || {};
        const resp = d.response;
        const status = resp ? `${resp.status} ${resp.statusText}` : "(no response)";
        console.error(`[ApiDebuglog] ${context} failed`, {
          url: d.url,
          method: d.method,
          status,
          payload: safeClone(d.payload),
          result: safeClone(d.result),
          summary: summarizeResult(d.result),
        });
      } catch {
        // no-op
      }
  }

  function logCrash(context, details) {
      try {
        const d = details || {};
        console.error(`[ApiDebuglog] ${context} crashed`, {
          url: d.url,
          method: d.method,
          payload: safeClone(d.payload),
          error: d.error,
        });
      } catch {
        // no-op
      }
  };

  // Simple helper: call this in failure paths.
  // type: 'failure' | 'crash'
  function handleApiError(type, context, details) {
    if (type === "crash") {
      logCrash(context, details);
    } else {
      logFailure(context, details);
    }
  }

  // Expose as global name you requested.
  // - ApiDebuglog: advanced usage
  // - handleApiError: simplest usage
  window.ApiDebuglog = { logFailure, logCrash };
  window.handleApiError = handleApiError;

  // Backwards compatibility (so older pages won’t break).
  window.adminApiDebug = window.ApiDebuglog;
})();

