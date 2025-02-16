using DesolaDomain.Entities.PageEntity;
using MediatR;

namespace DesolaServices.Commands.Requests;
public class UpdateWebSectionCommand : IRequest
{
    public WebSection WebSection { get; }

    public UpdateWebSectionCommand(WebSection webSection)
    {
        WebSection = webSection;
    }
}