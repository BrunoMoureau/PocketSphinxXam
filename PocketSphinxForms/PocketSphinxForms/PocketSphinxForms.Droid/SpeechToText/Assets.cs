using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Android.Content.Res;
using System.Threading.Tasks;

namespace PocketSphinxForms.Droid.SpeechToText
{
    public class Assets
    {
        public static string ASSET_LIST_NAME = "assets.lst";

        public static string SYNC_DIR = "sync";
        public static string HASH_EXT = ".md5";

        private AssetManager assetManager;
        public File externalDir { get; }

        public Assets(Context context)
        {
            File appDir = context.GetExternalFilesDir(null); // get root directory in android where app is installed
            if (appDir == null)
                throw new IOException("Cannot get external files dir, external storage state is " + Android.OS.Environment.ExternalStorageState);

            externalDir = new File(appDir.AbsolutePath + File.Separator + SYNC_DIR);
            externalDir.Mkdir();
            //externalDir = new File(appDir, SYNC_DIR);

            assetManager = context.Assets;
        }

        public Assets(Context context, String dest)
        {
            externalDir = new File(dest);
            assetManager = context.Assets;
        }

        private List<String> readLines(System.IO.Stream source)
        {
            List<string> lines = new List<string>();
            BufferedReader br = new BufferedReader(new InputStreamReader(source));
            string line;
            while (null != (line = br.ReadLine()))
                lines.Add(line);
            return lines;
        }

        private System.IO.Stream openAsset(string asset)
        {
            return assetManager.Open(new File(SYNC_DIR, asset).Path);
        }

        public Dictionary<string, string> getItems()
        {
            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (string path in readLines(openAsset(ASSET_LIST_NAME)))
            {
                Reader reader = new InputStreamReader(openAsset(path + HASH_EXT));
                items.Add(path, new BufferedReader(reader).ReadLine());
            }
            return items;
        }

        public async Task<Dictionary<string, string>> getExternalItems()
        {
            try
            {
                Dictionary<string, string> items = new Dictionary<string, string>();
                File assetFile = new File(externalDir, ASSET_LIST_NAME);

                var reader = new System.IO.StreamReader(assetFile.Path);
                var content = await reader.ReadToEndAsync();
                var lines = content.Split('\n');
                
                foreach (string line in lines)
                {
                    if (!line.Equals(String.Empty))
                    {
                        String[] fields = line.Split(' ');
                        items.Add(fields[0], fields[1]);
                    }
                }
                return items;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public async Task Copy(string asset)
        {
            System.IO.Stream source = openAsset(asset);
            File destinationFile = new File(externalDir, asset);
            destinationFile.ParentFile.Mkdir();

            OutputStream destination = new FileOutputStream(destinationFile);
            byte[] buffer = new byte[1024];
            int nread;

            System.Diagnostics.Debug.WriteLine(this.GetType().Name, $"Copying asset {destinationFile} to {asset}");

            while ((nread = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await destination.WriteAsync(buffer, 0, nread);
                await destination.FlushAsync();
            }
            
            destination.Close();

            System.Diagnostics.Debug.WriteLine(this.GetType().Name, $"DONE! Copied asset {asset} to {destinationFile}");
        }

        public async Task updateItemList(Dictionary<string, string> items)
        {
            File assetListFile = new File(externalDir, ASSET_LIST_NAME);
            PrintWriter pw = new PrintWriter(new File(assetListFile.Path));
            foreach (var entry in items)
            {
                await pw.PrintlnAsync($"{entry.Key} {entry.Value}");
                System.Diagnostics.Debug.WriteLine(this.GetType().Name, $"Wrinting {entry.Key} {entry.Value} in {assetListFile.Path}");
            }
            pw.Close();
        }

        public async Task<File> syncAssets()
        {
            List<string> newItems = new List<string>();
            List<string> unusedItems = new List<string>();
            Dictionary<string, string> items = getItems();
            Dictionary<string, string> externalItems = await getExternalItems();

            foreach (string path in items.Keys)
            {
                if (externalItems.ContainsKey(path) && externalItems[path].Equals(items[path]))
                {
                    System.Diagnostics.Debug.WriteLine(this.GetType().Name, $"Skipping asset {path}: checksums are equal");
                }
                else
                {
                    newItems.Add(path);
                }
            }

            unusedItems.AddRange(externalItems.Keys);
            unusedItems.RemoveAll(item => items.Keys.Contains(item));

            int i = 0;
            Task[] tasks = new Task[newItems.Count];
            foreach (string path in newItems)
            {
                tasks[i] = await Task.Factory.StartNew(async () => await Copy(path));
                i++;
            }

            Task.WaitAll(tasks);

            //Parallel.ForEach(newItems, async path =>
            //{
            //    File file = await Copy(path);
            //    System.Diagnostics.Debug.WriteLine(this.GetType().Name, $"Copying asset {path} to {file}");
            //});

            //foreach (string path in newItems)
            //{
            //    File file = await Copy(path);
            //    System.Diagnostics.Debug.WriteLine(this.GetType().Name, $"Copying asset {path} to {file}");
            //}

            foreach (string path in unusedItems)
            {
                File file = new File(externalDir, path);
                file.Delete();
                System.Diagnostics.Debug.WriteLine(this.GetType().Name, $"Removing asset {file}");
            }

            await updateItemList(items);
            return externalDir;
        }
    }
}