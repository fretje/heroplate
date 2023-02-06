using MudBlazor;

namespace Heroplate.Admin.Infrastructure.Theme;

public class CustomTheme : MudTheme
{
    public CustomTheme()
    {
        Palette = new Palette()
        {
            Primary = "#F26963",
            Secondary = "#eaaf1a",
            SecondaryContrastText = "#636363",
            Info = "#1babee",
            Error = "#d32a1b",
            Background = "#f1f1f1",
            Success = "#75ba39",
            Surface = "#EEE7E9",
            SuccessContrastText = "#080808",
            TextPrimary = "#080808",
            AppbarBackground = "#EEE9E9",
            AppbarText = "#6e6e6e",
            DrawerBackground = "#EEE9E9",
            TableStriped = "#e9e1e1",
            TableHover = "#00000014",
            TableLines = "#27272f",
            DrawerText = "rgba(0,0,0, 0.7)",
            OverlayDark = "hsl(0deg 0% 0% / 75%)"
        };

        PaletteDark = new Palette()
        {
            Primary = "#F26963",
            Black = "#27272f",
            Background = "#2c2d34",
            BackgroundGrey = "#27272f",
            Surface = "#21212a",
            DrawerBackground = "#27272f",
            DrawerText = "rgba(255,255,255, 0.50)",
            DrawerIcon = "rgba(255,255,255, 0.50)",
            AppbarBackground = "#27272f",
            AppbarText = "rgba(255,255,255, 0.70)",
            TextPrimary = "rgba(255,255,255, 0.70)",
            TextSecondary = "rgba(255,255,255, 0.50)",
            ActionDefault = "#dcdce0",
            ActionDisabled = "#ffffff38",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",
            Divider = "rgba(255,255,255, 0.12)",
            DividerLight = "rgba(96, 94, 94, 1)",
            TableLines = "rgba(255,255,255, 0.12)",
            TableStriped = "#242028",
            TableHover = "#202428",
            LinesDefault = "rgba(255,255,255, 0.12)",
            LinesInputs = "rgba(255,255,255, 0.3)",
            TextDisabled = "#ffffff38",
        };

        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "5px"
        };

        Typography = CustomTypography.HeroTypography;
        Shadows = new Shadow();
        ZIndex = new ZIndex() { Drawer = 1300 };
    }
}