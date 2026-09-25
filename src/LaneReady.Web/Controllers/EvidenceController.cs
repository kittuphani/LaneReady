using LaneReady.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaneReady.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/evidence")]
public class EvidenceController : ApiControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IBlobStorageService _blobStorage;
    private readonly ICurrentUserService _currentUser;

    public EvidenceController(
        IApplicationDbContext dbContext,
        IBlobStorageService blobStorage,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _blobStorage = blobStorage;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Generates a short-lived SAS URI for an evidence document and returns a 302 redirect.
    /// The SAS URL is never stored client-side (OWASP A04).
    /// </summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.HasOrganisation)
            return Unauthorized();

        var doc = await _dbContext.EvidenceDocuments.FindAsync([id], cancellationToken);
        if (doc is null) return NotFound();

        // OrganisationId check enforced by EF global query filter on FindAsync
        var sasUri = await _blobStorage.GenerateDownloadSasUriAsync(
            doc.BlobContainerName,
            doc.BlobPath,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return Redirect(sasUri.ToString());
    }
}
