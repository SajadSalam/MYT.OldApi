using Events.DATA.DTOs.File;

namespace Events.Services
{
    public interface IFileService
    {
        Task<(string? file, string? error)> Upload(FileForm fileForm);
        Task<(List<string>? files, string? error)> Upload(MultiFileForm filesForm);
    }

    public class FileService : IFileService
    {
        public async Task<(string? file, string? error)> Upload(FileForm fileForm)
        {
            try
            {
                var id = Guid.NewGuid();
                var extension = Path.GetExtension(fileForm.File.FileName);
                var fileName = $"{id}{extension}";

                var attachmentsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments");
                if (!Directory.Exists(attachmentsDir)) Directory.CreateDirectory(attachmentsDir);

                var path = Path.Combine(attachmentsDir, fileName);
                await using var stream = new FileStream(path, FileMode.Create);
                await fileForm.File.CopyToAsync(stream);

                var filePath = Path.Combine("Attachments", fileName);
                return (filePath, null);
            }
            catch (Exception e)
            {
                return (null, $"Error in uploading file: {e.Message}");
            }
        }

        public async Task<(List<string>? files, string? error)> Upload(MultiFileForm filesForm)
        {
            try
            {
                var fileList = new List<string>();
                foreach (var file in filesForm.Files)
                {
                    var (fileToAdd, error) = await Upload(new FileForm { File = file });
                    if (error != null)
                    {
                        return (null, error);
                    }

                    fileList.Add(fileToAdd!);
                }

                return (fileList, null);
            }
            catch (Exception e)
            {
                return (null, $"Error in uploading multiple files: {e.Message}");
            }
        }
    }
}