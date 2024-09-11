using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ZLibNet;
using Python.Runtime;

namespace GrabbedRandomizer
{
    public partial class Form1 : Form
    {
        public string extractDirectory = Environment.CurrentDirectory + "/extracted/";
        public string scriptDirectory;
        public List<Stage> stages = new List<Stage>();
        public List<StageFile> allFiles = new List<StageFile>();
        public FileTypes fileTypes = new FileTypes();
        public List<FileGroup> groups = new List<FileGroup>();

        public string[] groupsTypes =
        {
            "Imp", "Skeleton", "Mummy", "Spider", "JessieClyde", "Worm", "Medusa", "Zombie", "Pirate", "HauntedChair", "HauntedPicture", "HauntedCoat", "HauntedTv", "HauntedDoor", "ImpNinja", "ImpFire", "GrimReaper", "MummyCursed", "Warlock", "Chicken", "ChickenVampire", "Hunchback", "Vampire", "PirateCaptain"
        };

        public class FileTypes
        {
            public List<(string, uint)> values = new List<(string, uint)>();

            public FileTypes()
            {
                values.Add(("actorattribs", 0x19));
                values.Add(("actorgoals", 0xB));
                values.Add(("aidlist", 0xE));
                values.Add(("anim", 0x2));
                values.Add(("animevents", 0x5));
                values.Add(("callout", 0xD));
                values.Add(("cutscene", 0x7));
                values.Add(("cutsceneevents", 0x8));
                values.Add(("fxemitter", 0x1A));
                values.Add(("fxparticle", 0x1B));
                values.Add(("fxrumble", 0x1C));
                values.Add(("fxcamshake", 0x1D));
                values.Add(("ghoulybox", 0x16));
                values.Add(("ghoulyspawn", 0x17));
                values.Add(("marker", 0xC));
                values.Add(("misc", 0xA));
                values.Add(("model", 0x4));
                values.Add(("script", 0x18));
                values.Add(("texture", 0x1));
            }
        }

        public class Stage
        {
            public uint[] header = new uint[8];
            public byte[] bytes = new byte[8];
            public string name = "";
            public string outputFolder = "";
            public uint fileCount = 0;
            public List<string> enemies = new List<string>();
        }

        public class StageFile
        {
            public uint[] header = new uint[8];
            public string name = "";
            public uint size = 0;
            public bool data = false;
            public string path = "";
        }

        public class FileGroup
        {
            public string name = "";
            public List<StageFile> files = new List<StageFile>();

            public FileGroup(string name, List<StageFile> files)
            {
                this.name = name;
                this.files = files;
            }
        }

        public Form1()
        {
            InitializeComponent();
            FormClosing += YourForm_FormClosing;
            scriptDirectory = extractDirectory + "bundles/aid_script/";
        }

        private void YourForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Directory.Exists(extractDirectory))
            {
                foreach (var file in Directory.GetFiles(extractDirectory, "*.*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }
                Directory.Delete(extractDirectory, true);
            }
        }

        private async void Select_ISO_Button_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(extractDirectory))
                Directory.CreateDirectory(extractDirectory);
            var result = openFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                await XISOManager("-x", openFileDialog1.FileName);
            }
            else
            {
                return;
            }

            List<Task> tasks = new List<Task>();

            foreach (string path in Directory.GetFiles(scriptDirectory))
            {
                if (path.Contains("chapter") && path.Contains("playcam") && !path.Contains(".dec"))
                {
                    decompressLabel.Text = $"Decompressing: {Path.GetFileName(path)}";
                    //await BNLDecompress(path);
                }
            }

            await BNLDecompress(scriptDirectory + "ghoulies_chapter1_scene1_2playcam.bnl");
            await BNLDecompress(scriptDirectory + "ghoulies_chapter3a_scene2_1playcam.bnl");

            //PopulateGroups();

            BNLCompress(stages[0]);
            Cleanup(scriptDirectory);
            await XISOManager("-c", Environment.CurrentDirectory + "/grabbed_extracted");

            //saveFileDialog1.FileName = "grabbed_compressed";
            //saveFileDialog1.ShowDialog();

            //XISOManager("-c", saveFileDialog1.FileName);
        }

        public void Cleanup(string dir)
        {
            string[] subDirectories = Directory.GetDirectories(dir);

            // Loop through each subdirectory and delete them
            foreach (string subDir in subDirectories)
            {
                try
                {
                    Directory.Delete(subDir, true); // Delete the subdirectory recursively
                    Console.WriteLine($"Deleted directory: {subDir}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting directory {subDir}: {ex.Message}");
                }
            }


            string[] fileExtensions = { ".dec", ".recom" }; // Change these to your desired file extensions

            foreach (string extension in fileExtensions)
            {
                // Get files for each extension and concatenate the results
                string[] filesToDelete = Directory.GetFiles(dir, $"*{extension}");

                foreach (string file in filesToDelete)
                {
                    try
                    {
                        File.Delete(file); // Delete the file
                        Console.WriteLine($"Deleted file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting file {file}: {ex.Message}");
                    }
                }
            }


        }

        public async Task XISOManager(string arg, string filePath)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            string domain = @Environment.CurrentDirectory;
            startInfo.WorkingDirectory = domain;
            startInfo.FileName = "cmd.exe";

            if (arg == "-x")
            {
                startInfo.Arguments = $"/c xiso -d \"{extractDirectory}\" -x \"{filePath}\"";
            }
            else if (arg == "-c")
            {
                startInfo.Arguments = $"/c xiso -c ./extracted \"{filePath}.iso\"";
            }

            Debug.WriteLine(startInfo.Arguments);

            process.StartInfo = startInfo;
            process.Start();

            // Wait asynchronously for the process to exit
            await process.WaitForExitAsync();
        }

        public void PopulateGroups()
        {
            string str = "Skeleton";
                decompressLabel.Text = $"Populating: {str}";
                string stru = str.ToLower();
                List<StageFile> all;
                switch (stru)
                {
                    case "imp":
                        all = allFiles.Where(c => c.name.Contains(stru) && !c.name.Contains("impninja") && !c.name.Contains("impfire") && !c.name.Contains("impact")).ToList();
                        break;

                    case "mummy":
                        all = allFiles.Where(c => c.name.Contains(stru) && !c.name.Contains("mummycursed")).ToList();
                        break;

                    case "chicken":
                        all = allFiles.Where(c => c.name.Contains(stru) && !c.name.Contains("chickenvampire")).ToList();
                        break;

                    case "vampire":
                        all = allFiles.Where(c => c.name.Contains(stru) && !c.name.Contains("chickenvampire")).ToList();
                        break;

                    default:
                        all = allFiles.Where(c => c.name.Contains(stru)).ToList();
                        break;

                }
                string[] strs = allFiles.Where(c => all.Where(d => d == c).ToArray().Length == 0).Select(e => e.name).ToArray();
                foreach (StageFile file in all.ToList())
                {
                    foreach (string str2 in strs)
                    {
                        if (ContainsMultiLineStringInFileStream(new FileStream(file.path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, useAsync: true), str2))
                        {
                            Debug.WriteLine(str2);
                            all.Add(allFiles.First(c => c.name == str2));
                        }
                    }
                }
                groups.Add(new FileGroup(str, all));

            foreach (FileGroup group in groups)
            {
                string[] strs2 = group.files.Select(e => e.name).ToArray();
                File.WriteAllLines(Environment.CurrentDirectory + $"/txt/{group.name}.txt", strs2);
            }
        }

        public async Task BNLDecompress(string path)
        {
            Stage stage = new Stage();
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true);
            uint[] header = new uint[8];
            byte[] temp = new byte[4];

            file.Position = 0;
            for (int i = 0; i < 8; i++)
            {
                await file.ReadAsync(temp, 0, temp.Length);
                header[i] = BitConverter.ToUInt32(temp, 0);
                Debug.WriteLine(header[i]);
            }
            stage.header = header;
            stage.name = path;
            uint offset = header[3] + header[5] + header[7];
            uint offset1 = header[3];
            uint offset2 = header[3] + header[5];
            uint filecount = header[3] / 160;
            stage.fileCount = filecount;
            file.Position = 40;
            FileStream newFile = new FileStream(path + ".dec", FileMode.Create, FileAccess.ReadWrite, FileShare.Read, bufferSize: 4096, useAsync: true);
            byte[] data = new byte[(int)(file.Length - file.Position)];
            await file.ReadAsync(data, 0, data.Length);
            byte[] decompressedBytes;
            using (MemoryStream compressedStream = new MemoryStream(data))
            using (ZLibStream zlibStream = new ZLibStream(compressedStream, CompressionMode.Decompress))
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                await zlibStream.CopyToAsync(decompressedStream);

                // Get the decompressed bytes
                decompressedBytes = decompressedStream.ToArray();

                // Now, decompressedBytes contains the decompressed data
                Console.WriteLine("Decompressed data: " + BitConverter.ToString(decompressedBytes));
            }
            await newFile.WriteAsync(decompressedBytes, 0, decompressedBytes.Length);
            newFile.Close();
            file.Close();
            file = new FileStream(path + ".dec", FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            string newDir = path.Substring(0, path.Length - 4) + "/";
            stage.outputFolder = newDir;
            Directory.CreateDirectory(newDir);
            for (int i = 0; i < filecount; i++)
            {
                file.Position = i * 160;
                temp = new byte[128];
                await file.ReadAsync(temp, 0, 128);
                temp = FilterNullBytes(temp);
                string name = Encoding.UTF8.GetString(temp);
                if(allFiles.Where(c => c.name == name).ToList().Count != 0)
                {
                    StageFile sf = allFiles.First(c => c.name == name);
                    File.Copy(sf.path, newDir + name + ".info");
                    if(sf.data)
                    {
                        File.Copy(sf.path.Substring(0, sf.path.Length - 5) + ".data", newDir + name + ".data");
                    }
                    continue;
                }
                temp = new byte[4];
                uint[] fileHeader = new uint[8];
                for (int j = 0; j < 8; j++)
                {
                    await file.ReadAsync(temp, 0, temp.Length);
                    fileHeader[j] = BitConverter.ToUInt32(temp, 0);
                    Debug.WriteLine(header[j]);
                }
                StageFile stageFile = new StageFile();
                stageFile.header = fileHeader;
                stageFile.name = name;
                foreach(string str in groupsTypes)
                {
                    if(name.Contains(str.ToLower()) && !stage.enemies.Contains(str))
                    {
                        int index = name.IndexOf(str.ToLower());
                        if (name.Length == index + str.Length)
                            stage.enemies.Add(str);
                    }
                }
                stageFile.path = newDir + name + ".info";
                newFile = new FileStream(newDir + name + ".info", FileMode.Create, FileAccess.ReadWrite, FileShare.Read, bufferSize: 4096, useAsync: true);
                file.Position = offset + fileHeader[4];
                temp = new byte[fileHeader[5]];
                await file.ReadAsync(temp, 0, (int)fileHeader[5]);
                await newFile.WriteAsync(temp, 0, temp.Length);
                newFile.Close();
                file.Position = offset1 + fileHeader[6];
                if (fileHeader[7] > 0)
                {
                    stageFile.data = true;
                    newFile = new FileStream(newDir + name + ".data", FileMode.Create, FileAccess.ReadWrite, FileShare.Read, bufferSize: 4096, useAsync: true);
                    uint[] tempHeader = new uint[2];
                    temp = new byte[4];
                    await file.ReadAsync(temp, 0, temp.Length);
                    tempHeader[0] = BitConverter.ToUInt32(temp, 0);
                    await file.ReadAsync(temp, 0, temp.Length);
                    tempHeader[1] = BitConverter.ToUInt32(temp, 0);
                    uint tell = 0;
                    for (int j = 0; j < tempHeader[1]; j++)
                    {
                        temp = new byte[4];
                        await file.ReadAsync(temp, 0, temp.Length);
                        uint off = BitConverter.ToUInt32(temp, 0);
                        await file.ReadAsync(temp, 0, temp.Length);
                        uint size = BitConverter.ToUInt32(temp, 0);
                        tell = (uint)file.Position;
                        file.Position = offset2 + off;
                        temp = new byte[size];
                        await file.ReadAsync(temp, 0, temp.Length);
                        await newFile.WriteAsync(temp, 0, temp.Length);
                        file.Position = tell;
                    }
                    newFile.Close();
                }
                if (allFiles.Where(c => c.name == stageFile.name).ToArray().Length == 0)
                    allFiles.Add(stageFile);
            }
            stages.Add(stage);
            file.Close();
        }

        public static bool ContainsMultiLineStringInFileStream(Stream fileStream, string searchString)
        {
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string fileContent = reader.ReadToEnd();
                // Use Regex to perform a multi-line search for the target string
                Regex regex = new Regex(searchString, RegexOptions.Multiline);
                return regex.IsMatch(fileContent);
            }
        }

        public void AddGroup(string path, string group)
        {
            StageFile[] files = allFiles.Where(c => File.ReadAllLines(Environment.CurrentDirectory + $"/txt/{group}.txt").Where(d => c.name.Contains(d) && c.name.Length == d.Length).ToArray().Length != 0).ToArray();
            foreach (StageFile file in files)
            {
                if (File.Exists(path + file.name + ".info"))
                    continue;
                File.Copy(file.path, path + file.name + ".info");
                if (file.data)
                {
                    File.Copy(file.path.Substring(0, file.path.Length - 5) + ".data", path + file.name + ".data");
                }
            }
        }

        public void ReplaceMonster(string dir, string oldMon, string newMon)
        {
            AddGroup(dir, newMon);
            string[] files = Directory.GetFiles(dir);
            files = files.Where(c => Path.GetFileNameWithoutExtension(c).Contains("marker") || Path.GetFileNameWithoutExtension(c).Contains("script") || Path.GetFileNameWithoutExtension(c).Contains("ghoulybox") || Path.GetFileNameWithoutExtension(c).Contains("ghoulyspawn")).ToArray();
            string fresh1 = oldMon;
            string fresh2 = newMon;
            foreach (string file in files)
            {
                oldMon = fresh1;
                newMon = fresh2;
                for (int i = 0; i < 2; i++)
                {
                    if (i == 1)
                    {
                        oldMon = "aid_objparams_ghoulies_actor_" + oldMon.ToLower();
                        newMon = "aid_objparams_ghoulies_actor_" + newMon.ToLower();
                    }
                    int totalRead = 0;
                    using (FileStream fileStream = File.Open(file, FileMode.Open))
                    {
                        while (totalRead < fileStream.Length)
                        {
                            byte[] bytes = new byte[fileStream.Length - totalRead];
                            fileStream.Position = totalRead;
                            fileStream.Read(bytes, 0, bytes.Length);
                            string bString = Encoding.UTF8.GetString(bytes);
                            int index = bString.IndexOf(oldMon);
                            if (index > 0)
                            {
                                fileStream.Position = index + totalRead;
                                fileStream.Write(Encoding.UTF8.GetBytes(newMon).Concat(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }).ToArray(), 0, newMon.Length + 10);
                                totalRead = (int)fileStream.Position;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }



        public void BNLCompress(Stage stage)
        {
            ReplaceMonster(stage.outputFolder, "Imp", "HauntedTv");
            FileStream compressed = File.Create(stage.name.Substring(0, stage.name.Length - 4) + ".recom");
            string[] files = Directory.GetFiles(stage.outputFolder, "*.*", SearchOption.AllDirectories);
            Array.Sort(files);
            List<StageFile> stageFiles = new List<StageFile>();
            foreach (string file in files)
            {
                StageFile tempFile = new StageFile();
                FileStream fileStream = File.Open(file, FileMode.Open);
                tempFile.name = Path.GetFileName(file);
                tempFile.size = (uint)fileStream.Length;
                if (file.Contains("data"))
                    tempFile.data = true;
                stageFiles.Add(tempFile);
                fileStream.Close();
            }

            uint offset1 = (uint)stageFiles.Where(c => c.name.Contains(".info")).ToArray().Length * 160;
            List<StageFile> data = stageFiles.Where(c => c.data).ToList().OrderBy(c => c.name).ToList();
            uint totalSize = 0;
            uint splitTotal = 0;
            for (int i = 0; i < data.Count; i++)
            {
                int splitVar = data[i].name.Contains("background") ? 250000 : 66000;
                uint split = 1;
                byte[] dataInfo = BitConverter.GetBytes(8 + (split * 8));
                dataInfo = dataInfo.Concat(new byte[] { 0x1, 0x0, 0x0, 0x0 }).ToArray();
                dataInfo = dataInfo.Concat(BitConverter.GetBytes(totalSize)).ToArray();
                dataInfo = dataInfo.Concat(BitConverter.GetBytes(data[i].size)).ToArray();
                totalSize += data[i].size;

                stageFiles.Where(c => c.name.Contains(data[i].name.Substring(0, data[i].name.Length - 5)) && c.name.Contains("info")).ToArray()[0].header[6] = (uint)(i * 0x10);
                compressed.Position = offset1 + (i * 8) + (splitTotal * 8);
                compressed.Write(dataInfo, 0, dataInfo.Length);
                splitTotal += split;
            }
            totalSize = 0;
            uint offset2 = offset1 + (uint)((data.Count * 8) + (splitTotal * 8));
            for (int i = 0; i < data.Count; i++)
            {
                FileStream fileStream = File.Open(stage.outputFolder + data[i].name, FileMode.Open);
                byte[] dataBytes;
                dataBytes = new byte[data[i].size];
                fileStream.Read(dataBytes, 0, dataBytes.Length);
                compressed.Write(dataBytes, 0, dataBytes.Length);
                totalSize += (uint)dataBytes.Length;
                fileStream.Close();
            }
            long offset = offset2 + totalSize;
            stage.header[3] = offset1;
            stage.header[5] = offset2 - offset1;
            stage.header[7] = totalSize;
            data = stageFiles.Where(c => !c.data).ToList().OrderBy(c => c.name).ToList();
            stage.header[0] = 0x10000 + (uint)data.Count;
            totalSize = 0;
            for (int i = 0; i < data.Count; i++)
            {
                data[i].header[0] = fileTypes.values.Where(c => c.Item1 == gSubstring(data[i].name)).ToArray()[0].Item2;
                data[i].header[1] = allFiles.Where(c => data[i].name.Contains(c.name) && data[i].name.Substring(0, data[i].name.Length - 5).Length == c.name.Length).ToArray()[0].header[1];
                data[i].header[2] = 0x48E58836;
                data[i].header[3] = 2;
                data[i].header[4] = totalSize;
                data[i].header[7] = allFiles.Where(c => data[i].name.Contains(c.name) && data[i].name.Substring(0, data[i].name.Length - 5).Length == c.name.Length).ToArray()[0].header[7];
                compressed.Position = offset + totalSize;
                FileStream fileStream = File.Open(stage.outputFolder + data[i].name, FileMode.Open);
                byte[] dataBytes = new byte[fileStream.Length];
                fileStream.Read(dataBytes, 0, dataBytes.Length);
                compressed.Write(dataBytes, 0, dataBytes.Length);
                data[i].header[5] = (uint)dataBytes.Length;
                totalSize += (uint)dataBytes.Length;
                compressed.Position = i * 160;
                byte[] nameBytes = Encoding.UTF8.GetBytes(data[i].name.Substring(0, data[i].name.Length - 5));
                compressed.Write(nameBytes, 0, nameBytes.Length);
                compressed.Position = (i * 160) + 0x80;
                for (int j = 0; j < 8; j++)
                {
                    compressed.Write(BitConverter.GetBytes(data[i].header[j]), 0, 4);
                }
                fileStream.Close();
            }
            stage.header[6] = offset2 + 0x28;
            File.Delete(stage.name);
            FileStream newFile = File.Create(stage.name);
            for (int i = 0; i < 8; i++)
            {
                newFile.Write(BitConverter.GetBytes(stage.header[i]), 0, 4);
            }
            byte[] firstHalf = new byte[4];
            firstHalf = BitConverter.GetBytes(stage.header[7] + stage.header[6]);
            byte[] secHalf = new byte[4];
            secHalf = BitConverter.GetBytes((uint)(compressed.Length - offset));
            stage.bytes = firstHalf.Concat(secHalf).ToArray();
            newFile.Write(stage.bytes, 0, 8);
            newFile.Close();
            compressed.Close();
            pyCompress(stage.name);
        }

        public byte[] FilterNullBytes(byte[] inputArray)
        {
            // Count non-null bytes
            int nonNullCount = 0;
            foreach (byte b in inputArray)
            {
                if (b != 0)
                {
                    nonNullCount++;
                }
            }

            // Create a new array without null bytes
            byte[] filteredArray = new byte[nonNullCount];
            int index = 0;
            foreach (byte b in inputArray)
            {
                if (b != 0)
                {
                    filteredArray[index] = b;
                    index++;
                }
            }

            return filteredArray;
        }

        public string gSubstring(string str)
        {
            // Find the position of the first and second underscore
            int firstUnderscoreIndex = str.IndexOf('_');
            int secondUnderscoreIndex = str.IndexOf('_', firstUnderscoreIndex + 1);

            if (firstUnderscoreIndex != -1 && secondUnderscoreIndex != -1)
            {
                // Extract the word between the first and second underscore
                return str.Substring(firstUnderscoreIndex + 1, secondUnderscoreIndex - firstUnderscoreIndex - 1);
            }
            else
            {
                Console.WriteLine("String does not contain two underscores.");
                return "";
            }
        }

        public void pyCompress(string path)
        {
            Runtime.PythonDLL = AppContext.BaseDirectory + "\\python\\python312.dll";


            // Initialize the Python runtime
            PythonEngine.Initialize();

            using (Py.GIL()) // Acquire the Python Global Interpreter Lock
            {
                Py.Import("comp").InvokeMethod("comp", new PyObject[] { new PyString(path) });
            }

            // Shutdown the Python runtime
            PythonEngine.Shutdown();
        }

        private void CompressButton_Click(object sender, EventArgs e)
        {
            BNLCompress(stages[0]);
        }
    }
}