using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;
using Aspel.CoreFiscal.Cancelacion.Domain.Enums;
using System.Text;
using System.Xml.Linq;

namespace Aspel.CoreFiscal.Cancelacion.Infrastructure.ExternalServices.Adapters
{
    public class PegasoAdapter : IPacAdapter
    {
        public int PacId => 5; // IDPACKPEGASO heredado de C++

        private readonly IHttpClientFactory _httpClientFactory;

        public PegasoAdapter(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PacIntegrationResult> ExecuteCancellationAsync(CfdiDocument document, string xmlConfig50, string xmlDocBase64, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("PegasoClient");

            // 1. Decodificar y manipular el XML (Lógica transcrita de GeneraXmlPeticion)
            var xmlBytes = Convert.FromBase64String(xmlDocBase64);
            var rawXml = Encoding.UTF8.GetString(xmlBytes);

            var cleanedXml = rawXml
                .Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "")
                .Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>", "")
                .Replace("<?xml version=\"1.0\"?>", "");

            // Eliminar todo antes de <Cancelacion...
            int startIndex = cleanedXml.IndexOf("<Cancelacion", StringComparison.Ordinal);
            if (startIndex > 0)
            {
                cleanedXml = cleanedXml.Substring(startIndex);
            }

            var pegasoPayload = $"<CancelaCFD xmlns=\"http://cancelacfd.sat.gob.mx\">{cleanedXml}</CancelaCFD>";

            using var request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Content = new StringContent(pegasoPayload, Encoding.UTF8, "application/xml");

            var response = await client.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            // 2. Parseo de respuesta (ObtenInfoError de C++)
            int codigoError = 0;
            try
            {
                var xdoc = XDocument.Parse(responseText);
                var estatusUuid = xdoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Folio")?.Attribute("EstatusUUID")?.Value;

                if (!string.IsNullOrEmpty(estatusUuid))
                    int.TryParse(estatusUuid, out codigoError);
                else
                {
                    var codEstatus = xdoc.Root?.Attribute("CodEstatus")?.Value;
                    int.TryParse(codEstatus, out codigoError);
                }
            }
            catch
            {
                codigoError = -1; // XML Inválido devuelto por Pegaso
            }

            return MapSatErrorCode(codigoError, responseText);
        }

        private PacIntegrationResult MapSatErrorCode(int codigoError, string acuse)
        {
            // Transcripción de ObtenErrorSAT de C++
            return codigoError switch
            {
                201 or 1201 or 202 or 1202 => new PacIntegrationResult { IsSuccess = true, NewState = CfdiState.Canceled, Acuse = acuse },
                205 or 1205 => new PacIntegrationResult { IsSuccess = false, NewState = CfdiState.Rejected, ErrorMessage = "El UUID no existe en el SAT." },
                203 or 1203 => new PacIntegrationResult { IsSuccess = false, NewState = CfdiState.Rejected, ErrorMessage = "El UUID no corresponde con el emisor." },
                304 or 1308 => new PacIntegrationResult { IsSuccess = false, NewState = CfdiState.Rejected, ErrorMessage = "Certificado Revocado o Caduco." },
                _ => new PacIntegrationResult { IsSuccess = false, NewState = CfdiState.Rejected, ErrorMessage = $"Error del PAC o SAT. Código: {codigoError}" }
            };
        }
    }
}
