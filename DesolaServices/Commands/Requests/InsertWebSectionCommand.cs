using DesolaDomain.Entities.Pages;
using MediatR;

namespace DesolaServices.Commands.Requests;
public class InsertWebSectionCommand : IRequest
{
    public WebSection WebSection { get; }

    public InsertWebSectionCommand(WebSection webSection)
    {
        WebSection = webSection;
    }
}