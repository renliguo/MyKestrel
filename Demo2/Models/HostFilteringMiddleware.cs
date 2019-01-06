using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Demo2.Models
{
    public class HostFilteringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IList<string> _hosts;
        private readonly ILogger<HostFilteringMiddleware> _logger;

        public HostFilteringMiddleware(RequestDelegate next, IConfiguration config,
            ILogger<HostFilteringMiddleware> logger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // A semicolon separated list of host names without the port numbers.
            // IPv6 addresses must use the bounding brackets and be in their normalized form.
            _hosts = config["AllowedHosts"]?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (_hosts == null || _hosts.Count == 0)
            {
                throw new InvalidOperationException("No configuration entry found for AllowedHosts.");
            }
        }

        public Task Invoke(HttpContext context)
        {
            if (!ValidateHost(context))
            {
                context.Response.StatusCode = 400;
                _logger.LogDebug("Request rejected due to incorrect Host header.");
                return Task.CompletedTask;
            }

            return _next(context);
        }


        // This does not duplicate format validations that are expected to be performed by the host.
        private bool ValidateHost(HttpContext context)
        {
            StringSegment host = context.Request.Headers[HeaderNames.Host].ToString().Trim();
            if (StringSegment.IsNullOrEmpty(host))
            {
                // Http/1.0 does not require the Host header.
                // Http/1.1 requires the header but the value may be empty.
                return true;
            }

            // Drop the port
            int colonIndex = host.LastIndexOf(':');
            // IPv6 special case
            if (host.StartsWith("[", StringComparison.Ordinal))
            {
                int endBracketIndex = host.IndexOf(']');
                if (endBracketIndex < 0)
                {
                    // Invalid format
                    return false;
                }

                if (colonIndex < endBracketIndex)
                {
                    {
                        // No port, just the IPv6 Host
                        colonIndex = -1;
                    }
                }

                if (colonIndex > 0)
                {
                    host = host.Subsegment(0, colonIndex);
                }

                foreach (string allowedHost in _hosts)
                {
                    if (StringSegment.Equals(allowedHost, host, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Sub-domain wildcards: *.example.com
                    if (allowedHost.StartsWith("*.", StringComparison.Ordinal) && host.Length >=
                        allowedHost.Length)
                    {
                        // .example.com
                        StringSegment allowedRoot = new StringSegment(allowedHost, 1, allowedHost.Length - 1);
                        StringSegment hostRoot = host.Subsegment(host.Length - allowedRoot.Length, allowedRoot.Length);
                        if (hostRoot.Equals(allowedRoot, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            return true;
        }
    }
}

