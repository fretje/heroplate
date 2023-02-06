namespace Heroplate.Admin.Application.Catalog;

public class BrandAutocomplete : MudAutocomplete<int>
{
    [Inject]
    private IStringLocalizer<BrandAutocomplete> L { get; set; } = default!;
    [Inject]
    private IBrandsClient BrandsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<BrandDto> _brands = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = L["Brand"];
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchBrandsAsync;
        ToStringFunc = GetBrandName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (_value > 0)
            {
                if (await ApiHelper.ExecuteCallGuardedAsync(
                    () => BrandsClient.GetAsync(_value), Snackbar) is { } brand)
                {
                    _brands.Add(brand);
                    ForceRender(true);
                }
            }
        }
    }

    private async Task<IEnumerable<int>> SearchBrandsAsync(string value)
    {
        var filter = new SearchBrandsRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => BrandsClient.SearchAsync(filter), Snackbar)
            is { } response)
        {
            _brands = response.Data.ToList();
        }

        return _brands.Select(x => x.Id);
    }

    private string GetBrandName(int id) =>
        _brands.Find(b => b.Id == id) is { } brand
            ? brand.Name
            : "";
}