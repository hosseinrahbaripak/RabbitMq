using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NotificationSystem.Application.Abstractions.MessageBroker;

namespace NotificationSystem.Web.Pages;

public class IndexModel(IMessagePublisher messagePublisher) : PageModel
{


    public void OnGet()
    {

    }
    public async Task<IActionResult> OnPostAsync()
    {
        await messagePublisher.PublishAsync(new {Test="hello"}, "notification.queue");

        return Page();
    }

}
