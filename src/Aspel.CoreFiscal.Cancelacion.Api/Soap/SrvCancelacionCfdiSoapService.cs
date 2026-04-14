using Aspel.CoreFiscal.Cancelacion.Application.UseCases.Cancelacion;
using CoreWCF;
using MediatR;
using System.Text.Json;

namespace Aspel.CoreFiscal.Cancelacion.Api.Soap
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class SrvCancelacionCfdiSoapService : ISrvCancelacionCfdiSoap
    {
        private readonly IMediator _mediator;

        public SrvCancelacionCfdiSoapService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<string> CancelarCfdiAsync(string uuid, string rfcEmisor, string rfcReceptor, decimal total, string xmlDocBase64, string xmlConfig50)
        {
            var command = new CancelCfdiCommand(uuid, rfcEmisor, rfcReceptor, total, xmlDocBase64, xmlConfig50);

            // Ejecutamos el caso de uso a través de MediatR
            var result = await _mediator.Send(command);

            // Retornamos el resultado serializado (o adaptado según lo requiera el cliente C++ legacy)
            return JsonSerializer.Serialize(result);
        }
    }
}
