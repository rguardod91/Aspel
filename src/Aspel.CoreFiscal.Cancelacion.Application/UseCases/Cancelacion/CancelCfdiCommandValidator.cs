using FluentValidation;

namespace Aspel.CoreFiscal.Cancelacion.Application.UseCases.Cancelacion
{
    public class CancelCfdiCommandValidator : AbstractValidator<CancelCfdiCommand>
    {
        public CancelCfdiCommandValidator()
        {
            RuleFor(x => x.Uuid)
                .NotEmpty().WithMessage("El UUID es requerido.")
                .Matches(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
                .WithMessage("El formato del UUID no es válido.");

            RuleFor(x => x.RfcEmisor)
                .NotEmpty().WithMessage("El RFC Emisor es requerido.");

            RuleFor(x => x.RfcReceptor)
                .NotEmpty().WithMessage("El RFC Receptor es requerido.");

            RuleFor(x => x.XmlDocBase64)
                .NotEmpty().WithMessage("El documento XML en Base64 es requerido.");
        }
    }
}
