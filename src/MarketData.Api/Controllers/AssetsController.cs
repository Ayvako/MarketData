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

    [HttpGet("{id}/price")]
    public async Task<IActionResult> GetAssetPriceInfo(Guid id, CancellationToken ct)
    {
        var priceInfo = await this.assetService.GetAssetPriceInfoAsync(id, ct).ConfigureAwait(false);

        if (priceInfo == null)
        {
            return this.NotFound(new { Message = "Asset not found" });
        }

        return this.Ok(priceInfo);
    }
}