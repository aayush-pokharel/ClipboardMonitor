using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardIntercept.Common
{
    public class FileWriteUtility
    {

        /// <summary>
        /// Insert text to the top of a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task InsertTextToTop(string path, string newText)
        {
            //if there is no existing file write text directly to new file
            if (!File.Exists(path))
            {
                System.IO.FileInfo file = new System.IO.FileInfo(path);
                //Create the file directory if the directory does not exist
                file.Directory?.Create();

                await File.WriteAllTextAsync(path, newText);
                return;
            }
            //path validity check
            var pathDir = Path.GetDirectoryName(path);
            if (String.IsNullOrEmpty(pathDir))
                throw new Exception($"{pathDir} does not exist");


            var tempPath = Path.Combine(pathDir, Guid.NewGuid().ToString("N"));
            //write new text to a temp file followed by end line terminator
            {
                await using var stream = new FileStream(tempPath, FileMode.Create,
                    FileAccess.Write, FileShare.None, 4 * 1024 * 1024);
                await using var sw = new StreamWriter(stream);
                await sw.WriteLineAsync(newText);
                sw.Flush();

                //copy the contents of the current clip file to the new temp file
                await using var old = File.OpenRead(path);
                await old.CopyToAsync(sw.BaseStream);
            }

            //replace old file with the new temp one created
            File.Delete(path);
            File.Move(tempPath, path);
        }
        /// <summary>
        /// Insert text to the bottom of a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newText"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AppendTextToFile(string path, string newText)
        {
            //if there is no existing file write text directly to new file
            if (!File.Exists(path))
            {
                System.IO.FileInfo file = new System.IO.FileInfo(path);
                //Create the file directory if the directory does not exist
                file.Directory?.Create();

                await File.WriteAllTextAsync(path, newText);
                return;
            }
            //path validity check
            var pathDir = Path.GetDirectoryName(path);
            if (String.IsNullOrEmpty(pathDir))
                throw new Exception($"{pathDir} does not exist");

            await using var stream = new FileStream(path, FileMode.Append,
                    FileAccess.Write, FileShare.None, 4 * 1024 * 1024);

            await using var sw = new StreamWriter(stream);
            await sw.WriteLineAsync(newText);
        }

        /// <summary>
        /// Trim top of file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task TrimFileTop(string filename, long bytes)
        {
            var fileSize = (new System.IO.FileInfo(filename)).Length;
            if (fileSize > bytes)
            {
                var text = await File.ReadAllTextAsync(filename);

                var amountToKeep = (int)(text.Length * 0.9);
                amountToKeep = text.IndexOf('\n', amountToKeep);
                var trimmedText = text.Substring(amountToKeep + 1);

                await File.WriteAllTextAsync(filename, trimmedText);
            }
        }
        /// <summary>
        /// Trim bottom of file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task TrimFileBottom(string filename, long bytes)
        {
            var fileSize = (new System.IO.FileInfo(filename)).Length;

            if (fileSize > bytes)
            {
                var text = await File.ReadAllTextAsync(filename);

                var amountToKeep = (int)(text.Length * 0.9);
                amountToKeep = text.IndexOf('\n', amountToKeep);
                var trimmedText = text.Substring(0, amountToKeep + 1);

                await File.WriteAllTextAsync(filename, trimmedText);
            }
        }
    }
}
