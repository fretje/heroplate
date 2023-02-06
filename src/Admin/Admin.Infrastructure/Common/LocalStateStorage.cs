using Blazored.LocalStorage;
using Fluxor.Persist.Storage;

namespace Heroplate.Admin.Infrastructure.Common;

public class LocalStateStorage : IStringStateStorage
{
    private ILocalStorageService LocalStorage { get; set; }

    public LocalStateStorage(ILocalStorageService localStorage) =>
        LocalStorage = localStorage;

    public async ValueTask<string> GetStateJsonAsync(string statename) =>
        await LocalStorage.GetItemAsStringAsync(statename);

    public async ValueTask StoreStateJsonAsync(string statename, string json) =>
        await LocalStorage.SetItemAsStringAsync(statename, json);
}