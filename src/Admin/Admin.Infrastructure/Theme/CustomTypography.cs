using MudBlazor;

namespace Heroplate.Admin.Infrastructure.Theme;

public static class CustomTypography
{
    public static Typography HeroTypography => new()
    {
        Default = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = ".875rem",
            FontWeight = 200,
            LineHeight = 1.43,
            LetterSpacing = ".01071em"
        },
        H1 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "6rem",
            FontWeight = 300,
            LineHeight = 1.167,
            LetterSpacing = "-.01562em"
        },
        H2 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "3.75rem",
            FontWeight = 300,
            LineHeight = 1.2,
            LetterSpacing = "-.00833em"
        },
        H3 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "3rem",
            FontWeight = 400,
            LineHeight = 1.167,
            LetterSpacing = "0"
        },
        H4 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "2.125rem",
            FontWeight = 400,
            LineHeight = 1.235,
            LetterSpacing = ".00735em"
        },
        H5 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "1.5rem",
            FontWeight = 400,
            LineHeight = 1.334,
            LetterSpacing = "0"
        },
        H6 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "1.25rem",
            FontWeight = 400,
            LineHeight = 1.6,
            LetterSpacing = ".0075em"
        },
        Button = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = ".875rem",
            FontWeight = 400,
            LineHeight = 1.75,
            LetterSpacing = ".02857em"
        },
        Body1 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "1rem",
            FontWeight = 400,
            LineHeight = 1.5,
            LetterSpacing = ".00938em"
        },
        Body2 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = ".875rem",
            FontWeight = 400,
            LineHeight = 1.43,
            LetterSpacing = ".01071em"
        },
        Caption = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = ".75rem",
            FontWeight = 200,
            LineHeight = 1.66,
            LetterSpacing = ".03333em"
        },
        Subtitle1 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = "1rem",
            FontWeight = 400,
            LineHeight = 1.57,
            LetterSpacing = ".00714em"
        },
        Subtitle2 = new()
        {
            FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" },
            FontSize = ".875rem",
            FontWeight = 400,
            LineHeight = 1.57,
            LetterSpacing = ".00714em"
        }
    };
}