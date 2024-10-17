using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace GatekeeperX.Middleware
{
    public class GatekeeperXValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public GatekeeperXValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method))
            {
                context.Request.EnableBuffering();
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(body) && !ValidateJsonInput(body))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Invalid input data.");
                        return;
                    }
                }
                context.Request.Body.Position = 0;
            }
            await _next(context);
        }

        private bool ValidateJsonInput(string json)
        {
            var parsedJson = JsonDocument.Parse(json).RootElement;
            foreach (var property in parsedJson.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    string value = property.Value.GetString();

                    // Verifica se o campo é uma URL e protege contra SSRF
                    if (Uri.IsWellFormedUriString(value, UriKind.Absolute) && !ValidateUrlForSSRF(value))
                    {
                        return false; // Bloqueia URLs suspeitas
                    }

                    // Proteção contra XSS e SQL Injection
                    if (HasXssRisk(value) || ValidateJsonInputForSqlInjection(value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        private bool ValidateUrlForSSRF(string url)
        {
            // Vê os componentes que compoêm a URL
            Uri uriResult;
            bool isUriValid = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!isUriValid)
            {
                return false; // URL inválido
            }

            // Lista de endereços locais e internos que queremos bloquear
            var localAddresses = new[]
            {
                "127.0.0.1", "localhost", "::1", "10.", "192.168.", "172.16.", "169.254."
            };

            foreach (var address in localAddresses)
            {
                if (uriResult.Host.StartsWith(address))
                {
                    return false; // URL interno, potencialmente perigoso
                }
            }

            return true; // URL válido
        }

        private bool HasXssRisk(string input)
        {
            // Regras simples de XSS, pode ser melhorado com uma biblioteca específica
            var blacklist = new[] { "<script>", "</script>", "javascript:" };
            return blacklist.Any(input.Contains);
        }

        private bool ValidateJsonInputForSqlInjection(string json)
        {
            try
            {
                var parsedJson = JsonDocument.Parse(json).RootElement;

                foreach (var property in parsedJson.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        string value = property.Value.GetString();
                        if (HasSqlInjectionRisk(value))
                        {
                            return false; // Bloquear a requisição se detectar um risco de SQL Injection
                        }
                    }
                }
            }
            catch (JsonException)
            {
                return false; // JSON inválido
            }

            return true; // Se tudo passar, o input é considerado seguro
        }

        private bool HasSqlInjectionRisk(string input)
        {
            // Lista de padrões e caracteres comuns em ataques de SQL Injection
            var blacklist = new[]
            {
                "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "EXEC",
                "--", ";", "/*", "*/", "@@", "CHAR(", "NCHAR(", "VARCHAR(", "NVARCHAR(",
                "ALTER", "BEGIN", "CAST", "CREATE", "DECLARE", "EXECUTE", "FETCH", "OPEN", "TRUNCATE"
            };

            // Verificar se qualquer padrão da lista aparece na entrada do usuário
            return blacklist.Any(item => input.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0);
        }

    }
}
