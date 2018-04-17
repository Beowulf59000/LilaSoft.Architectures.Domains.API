namespace LilaSoft.Architectures.Domains.API.Application.Validations
{
    using FluentValidation;
    using LilaSoft.Architectures.Domains.API.Application.Commands;
    using MediatR;

    public class IdentifiedCommandValidator : AbstractValidator<IdentifiedCommand<IRequest<bool>, bool>>
    {
        public IdentifiedCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
        }
    }
}
