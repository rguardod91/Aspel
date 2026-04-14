using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;
using Aspel.CoreFiscal.Cancelacion.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Xml.Linq;

namespace Aspel.CoreFiscal.Cancelacion.Infrastructure.ExternalServices.Adapters
{
    public class ComercioDigitalAdapter : IPacAdapter
    {
        public int PacId => 6; // IdPACComercioDigital heredado de C++

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _usuario;
        private readonly string _password;

        public ComercioDigitalAdapter(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            // Reemplaza Utileria::LeeIniConfig("6ComercioUsuarioCancel")
            _usuario = configuration["PacCredentials:ComercioDigital:Usuario"] ?? "";
            _password = configuration["PacCredentials:ComercioDigital:Password"] ?? "";
        }

        public async Task<PacIntegrationResult> ExecuteCancellationAsync(CfdiDocument document, string xmlConfig50, string xmlDocBase64, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("ComercioDigitalClient");

            // Decodificamos el XML original para extraer la petición pura
            var xmlBytes = Convert.FromBase64String(xmlDocBase64);
            var xmlString = Encoding.UTF8.GetString(xmlBytes);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/cancelar");
            request.Content = new StringContent(xmlString, Encoding.UTF8, "text/plain");

            // Cabeceras exigidas por Comercio Digital (Traducidas de InvocaProcesoRemoto)
            request.Headers.Add("usrws", _usuario);
            request.Headers.Add("pwdws", _password);
            request.Headers.Add("tipo", document.VersionCfdi >= 20 ? "reten" : "cfdi"); // Simplificación de strVersion
            request.Headers.Add("emaile", "");
            request.Headers.Add("emailr", "");
            request.Headers.Add("rfcr", document.RfcReceptor.Value);
            request.Headers.Add("total", document.Total.ToString("F2"));
            request.Headers.Add("tipocfdi", "I"); // Asumido Ingreso por defecto, requiere extracción del XML real

            var response = await client.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parseo de Errores basado en ObtenInfoError() de C++
            int codigoError = 0;
            string mensajeError = string.Empty;

            if (response.Headers.TryGetValues("codigo", out var codigos))
            {
                int.TryParse(codigos.FirstOrDefault(), out codigoError);
                if (response.Headers.TryGetValues("errmsg", out var mensajes))
                    mensajeError = mensajes.FirstOrDefault() ?? string.Empty;
            }
            else
            {
                // Parseo desde el XML de Acuse
                try
                {
                    var xdoc = XDocument.Parse(responseText);
                    var estatusUuid = xdoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Folio")?.Attribute("EstatusUUID")?.Value;
                    if (!string.IsNullOrEmpty(estatusUuid))
                    {
                        int.TryParse(estatusUuid, out codigoError);
                    }
                    else
                    {
                        var codEstatus = xdoc.Root?.Attribute("CodEstatus")?.Value;
                        int.TryParse(codEstatus, out codigoError);
                    }
                }
                catch
                {
                    // XML inválido, se mantiene codigoError en 0
                }
            }

            return MapSatErrorCode(codigoError, mensajeError, responseText);
        }

        private PacIntegrationResult MapSatErrorCode(int codigoError, string mensajeError, string acuse)
        {
            // Lógica transcrita de ObtenerErrorSAT() y ObtenInfoError()
            return codigoError switch
            {
                201 or 1201 or 202 or 1202 => new PacIntegrationResult
                {
                    IsSuccess = true,
                    NewState = CfdiState.Canceled,
                    Acuse = acuse
                },
                911 or 901 => new PacIntegrationResult
                {
                    IsSuccess = true,
                    NewState = CfdiState.Pending, // "En proceso de cancelación"
                    Acuse = acuse
                },
                205 or 1205 => new PacIntegrationResult
                {
                    IsSuccess = false,
                    NewState = CfdiState.Rejected,
                    ErrorMessage = "El UUID No existe en el SAT.",
                    IsHttpError = false
                },
                _ => new PacIntegrationResult
                {
                    IsSuccess = false,
                    NewState = CfdiState.Rejected,
                    ErrorMessage = string.IsNullOrEmpty(mensajeError) ? $"Error no tipificado: {codigoError}" : mensajeError,
                    IsHttpError = false
                }
            };
        }
    }
}
