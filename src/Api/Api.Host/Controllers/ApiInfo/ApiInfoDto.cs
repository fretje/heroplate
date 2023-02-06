namespace Heroplate.Api.Host.Controllers.ApiInfo;

public class ApiInfoDto
{
    public string Product { get; }
    public string Version { get; }
    public string GitSha { get; }

    public ApiInfoDto(string product, string version, string gitSha)
    {
        Product = product;
        Version = version;
        GitSha = gitSha;
    }
}