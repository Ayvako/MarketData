namespace MarketData.Api.Controllers;

using MarketData.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AssetsController(IAssetService assetService) : ControllerBase
{
    private readonly IAssetService assetService = assetService;

    [HttpGet]
    public async Task<IActionResult> GetAssets(CancellationToken ct)
    {
        var assets = await this.assetService.GetAllAssetsAsync(ct).ConfigureAwait(false);
        return this.Ok(assets);
    }
}