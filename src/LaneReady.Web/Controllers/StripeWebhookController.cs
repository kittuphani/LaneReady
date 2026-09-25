using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace LaneReady.Web.Controllers;

[ApiController]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("api/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly string _webhookSecret;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(IConfiguration configuration, ILogger<StripeWebhookController> logger)
    {
        _webhookSecret = configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured");
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed");
            return BadRequest("Invalid webhook signature");
        }

        _logger.LogInformation("Stripe webhook received: {EventType} {EventId}", stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
            case "invoice.payment_succeeded":
            case "invoice.payment_failed":
                // TODO: dispatch to MediatR handler in Sprint 9 (E9 Billing epic)
                break;
        }

        return Ok();
    }
}
