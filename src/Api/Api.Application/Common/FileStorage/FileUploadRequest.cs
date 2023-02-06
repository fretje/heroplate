namespace Heroplate.Api.Application.Common.FileStorage;

public class FileUploadRequest
{
    public string Name { get; set; } = default!;
    public string Extension { get; set; } = default!;
    public string Data { get; set; } = default!;
}

public class FileUploadRequestValidator : AbstractValidator<FileUploadRequest>
{
    public FileUploadRequestValidator(IStringLocalizer<FileUploadRequestValidator> localizer)
    {
        RuleFor(p => p.Name)
            .NotEmpty()
                .WithMessage(localizer["Image Name cannot be empty!"])
            .MaximumLength(150);

        RuleFor(p => p.Extension)
            .NotEmpty()
                .WithMessage(localizer["Image Extension cannot be empty!"])
            .MaximumLength(5);

        RuleFor(p => p.Data)
            .NotEmpty()
                .WithMessage(localizer["Image Data cannot be empty!"]);
    }
}