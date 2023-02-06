using Fluxor.Persist.Storage;

namespace Heroplate.Admin.Application.Layout;

[FeatureState(Name = nameof(LayoutState))]
[PersistState]
public record LayoutState(string LanguageCode, bool IsDarkMode, bool IsDrawerOpen)
{
    public LayoutState()
        : this("en-BE", false, true)
    {
    }
}

public record ChangeLanguageAction(string LanguageCode);
public record ChangeDarkModeAction(bool IsDarkMode);
public record ChangeDrawerStateAction(bool IsOpen);
public record ToggleDrawerStateAction();

public static class LayoutReducers
{
    [ReducerMethod]
    public static LayoutState ReduceChangeLanguage(LayoutState state, ChangeLanguageAction action) =>
        state with { LanguageCode = action.LanguageCode };

    [ReducerMethod]
    public static LayoutState ReduceChangeDarkMode(LayoutState state, ChangeDarkModeAction action) =>
        state with { IsDarkMode = action.IsDarkMode };

    [ReducerMethod]
    public static LayoutState ReduceChangeDrawerState(LayoutState state, ChangeDrawerStateAction action) =>
        state with { IsDrawerOpen = action.IsOpen };
}

public class LayoutEffects
{
    private readonly IState<LayoutState> _layoutState;
    public LayoutEffects(IState<LayoutState> layoutState) => _layoutState = layoutState;

    [EffectMethod(typeof(ToggleDrawerStateAction))]
    public Task OnToggleDrawerStateAsync(IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new ChangeDrawerStateAction(!_layoutState.Value.IsDrawerOpen));
        return Task.CompletedTask;
    }
}