using CoreWCF;

namespace Aspel.CoreFiscal.Cancelacion.Api.Soap
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface ISrvCancelacionCfdiSoap
    {
        [OperationContract]
        Task<string> CancelarCfdiAsync(string uuid, string rfcEmisor, string rfcReceptor, decimal total, string xmlDocBase64, string xmlConfig50);
    }
}
