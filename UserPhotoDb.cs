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
    public class UserPhotoDb : IUserPhotoDb
    {
        private const int SubdirectoryNameLength = 2;
        private const int StoredPhotoWidth = 96;
        private const int StoredPhotoHeight = 96;

        private readonly HashSet<string> _existingSubdirs;
        private readonly string _rootDir;

        public UserPhotoDb(string rootDir)
        {
            _rootDir = rootDir;
            // ensure directory exists
            System.IO.Directory.CreateDirectory(rootDir);

            _existingSubdirs = InitExistingSubdirs();
        }

        public void Put(string accountSid, byte[] imageBytes) 
        {
            using (Image<Rgba32> photo = Image.Load(imageBytes))
            {
                if (photo.Width != StoredPhotoWidth || photo.Height != StoredPhotoHeight)
                {
                    photo.Mutate(x => x.Resize(new ResizeOptions()
                    {
                        Size = new Size(96, 96),
                        Mode = ResizeMode.Stretch
                    }));
                }
                string filepath = GetFilePathForSid(accountSid);
                EnsureSubdirExistsForFile(filepath);
                using (var outfile = new System.IO.FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    photo.SaveAsJpeg(outfile);
                }
            }
        }

        public byte[] Get(string accountSid)
        {
            string filepath = GetFilePathForSid(accountSid);
            try
            {
                return System.IO.File.ReadAllBytes(filepath);
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
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

        private string GetFilePathForSid(string accountSid)
        {
            string normalizedSid = NormalizeAccountSid(accountSid);

            string subdirname = normalizedSid.Substring(normalizedSid.Length - SubdirectoryNameLength);
            string subdirpath = System.IO.Path.Combine(_rootDir, subdirname);
            
            string filepath = System.IO.Path.Combine(subdirpath, normalizedSid + ".jpg");
            return filepath;
        }

        private void EnsureSubdirExistsForFile(string filepath)
        {
            string subdirname = System.IO.Path.GetDirectoryName(filepath);
            var subdirpath = System.IO.Path.Combine(_rootDir, subdirname);
            
            if (!_existingSubdirs.Contains(subdirname))
            {
                System.IO.Directory.CreateDirectory(subdirpath);
                _existingSubdirs.Add(subdirname);
            }
        }

        private string NormalizeAccountSid(string accountSid)
        {
            return accountSid.ToLowerInvariant();
        }
    }
}