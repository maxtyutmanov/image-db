using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System.Collections.Generic;
using System.IO;

namespace image_db 
{
    public class UserPhotoDb 
    {
        private readonly HashSet<string> _existingSubdirs;
        private readonly string _rootDir;

        public UserPhotoDb(string rootDir)
        {
            _rootDir = rootDir;
            // ensure directory exists
            System.IO.Directory.CreateDirectory(rootDir);

            _existingSubdirs = InitExistingSubdirs();
        }

        private HashSet<string> InitExistingSubdirs()
        {
            var res = new HashSet<string>();

            foreach (string subdirFullPath in System.IO.Directory.EnumerateDirectories(_rootDir))
            {
                string subdirName = System.IO.Path.GetDirectoryName(subdirFullPath);
                res.Add(subdirName);
            }

            return res;
        }

        public void Store(string accountSid, string imageBase64) 
        {
            byte[] imageBytes = System.Convert.FromBase64String(imageBase64);

            using (Image<Rgba32> photo = Image.Load(imageBytes))
            {
                photo.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Size = new Size(96, 96),
                    Mode = ResizeMode.Stretch
                }));
                string dirPath = PrepareDirectoryForSid(accountSid);
                string outfilePath = System.IO.Path.Combine(dirPath, accountSid.ToLowerInvariant() + ".jpg");
                using (var outfile = new System.IO.FileStream(accountSid, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    photo.SaveAsJpeg(outfile);
                }
            }
        }

        private string PrepareDirectoryForSid(string accountSid)
        {
            var preparedSid = accountSid.ToLowerInvariant();
            var subdirname = preparedSid.Substring(preparedSid.Length - 2);

            var subdirpath = System.IO.Path.Combine(_rootDir, subdirname);

            if (!_existingSubdirs.Contains(subdirname))
            {
                System.IO.Directory.CreateDirectory(subdirpath);
                _existingSubdirs.Add(subdirname);
            }

            return subdirpath;
        }
    }
}