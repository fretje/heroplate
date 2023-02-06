namespace Heroplate.Admin.Application.Common;

public class HeroTable<T> : MudTable<T>
{
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Class = "px-4";
        Elevation = 25;
        Dense = true;
        Striped = true;
        Hover = true;
        return base.SetParametersAsync(parameters);
    }
}