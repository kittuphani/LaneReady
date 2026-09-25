using LaneReady.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using PlanTier = LaneReady.Domain.Enums.PlanTier;

namespace LaneReady.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        string organisationId,
        PlanTier plan,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var priceId = GetPriceId(plan);

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string> { ["organisationId"] = organisationId }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Created Stripe checkout session {SessionId} for org {OrgId} plan {Plan}",
            session.Id, organisationId, plan);

        return session.Url;
    }

    public async Task<string> CreateCustomerPortalSessionAsync(
        string stripeCustomerId,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = stripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return session.Url;
    }

    public Event ConstructWebhookEvent(string payload, string signatureHeader)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured");
        return EventUtility.ConstructEvent(payload, signatureHeader, webhookSecret);
    }

    private string GetPriceId(PlanTier plan) => plan switch
    {
        PlanTier.Starter => _configuration["Stripe:Prices:Starter"]
            ?? throw new InvalidOperationException("Stripe:Prices:Starter is not configured"),
        PlanTier.Growth => _configuration["Stripe:Prices:Growth"]
            ?? throw new InvalidOperationException("Stripe:Prices:Growth is not configured"),
        PlanTier.Enterprise => _configuration["Stripe:Prices:Enterprise"]
            ?? throw new InvalidOperationException("Stripe:Prices:Enterprise is not configured"),
        _ => throw new ArgumentOutOfRangeException(nameof(plan), plan, null)
    };
}
