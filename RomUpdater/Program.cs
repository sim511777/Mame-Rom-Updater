using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace RomUpdater
{
   class Program
   {
      private static StreamWriter stream = null;
      private static void LogOpen() {
         var exePath = Assembly.GetEntryAssembly().Location;
         var logPath = Path.GetDirectoryName(exePath) + "\\" + Path.GetFileNameWithoutExtension(exePath) + ".txt";
         stream = File.AppendText(logPath);
      }
      private static void LogClose() {
         if (stream != null)
            stream.Close();
      }
      private static void Log(string message) {
         Console.WriteLine(message);
         if (stream != null)
            stream.WriteLine(message);
      }
      private static void Log(string format, params object[] arg) {
         Console.WriteLine(format, arg);
         if (stream != null)
            stream.WriteLine(format, arg);
      }

      static void Main(string[] args)
      {
         LogOpen();

         if (args.Length != 2) {
            Log("Error : Invalid command parameter count");
            Log("usage: RomUpdate srcDir dstDir");
            return;
         }

         string srcDir = args[0];
         string dstDir = args[1];
         if (Directory.Exists(srcDir) == false) {
            Log("Error : Source Directory is not exists");
            Log("usage: RomUpdate srcDir dstDir");
            return;
         }
         if (Directory.Exists(dstDir) == false) {
            Log("Error : Destination Directory is not exists");
            Log("usage: RomUpdate srcDir dstDir");
            return;
         }
         
         var srcRomFiles = Directory.GetFiles(srcDir);
         Log("Rom Update from : {0}, file({1})", srcDir, srcRomFiles.Length);
         int copyCount = 0;
         int mergeCount = 0;
         int failCount = 0;
         for (int i=0; i<srcRomFiles.Length; i++) {
            var srcRomPath = srcRomFiles[i];
            string romFileName = Path.GetFileName(srcRomPath);
            string dstRomPath = dstDir + "\\" + romFileName;
            try {
               if (File.Exists(dstRomPath) == false) {
                  Log("({0}/{1}) {2} : Copy zip file", i+1, srcRomFiles.Length, romFileName);
                  File.Copy(srcRomPath, dstRomPath);
                  copyCount++;
               } else {
                  Log("({0}/{1}) {2} : Mrege zip file", i+1, srcRomFiles.Length, romFileName);
                  MergeZipFile(srcRomPath, dstRomPath);
                  mergeCount++;
               }
            } catch (Exception ex) {
               Log("Error : " + ex.Message);
               failCount++;
            }
         }

         Log("Rom Update finished : copy({0}), merge({1}), fail({2})", copyCount, mergeCount, failCount);

         LogClose();
      }

      private static void MergeZipFile(string srcPath, string dstPath) {
         string srcExtractDir = Path.GetDirectoryName(srcPath) + "\\" + Path.GetFileNameWithoutExtension(srcPath);
         string dstExtractDir = Path.GetDirectoryName(dstPath) + "\\" + Path.GetFileNameWithoutExtension(dstPath);;
         ZipFile.ExtractToDirectory(srcPath, srcExtractDir);
         ZipFile.ExtractToDirectory(dstPath, dstExtractDir);
         MergeDirectory(srcExtractDir, dstExtractDir);
         File.Delete(dstPath);
         ZipFile.CreateFromDirectory(dstExtractDir, dstPath);
         Directory.Delete(srcExtractDir, true);
         Directory.Delete(dstExtractDir, true);
      }

      private static void MergeDirectory(string srcDir, string dstDir) {
         foreach (var srcFilePath in Directory.GetFiles(srcDir)) {
            string fileName = Path.GetFileName(srcFilePath);
            string dstFilePath = dstDir + "\\" + fileName;
            File.Copy(srcFilePath, dstFilePath, true);
         }
      }
   }
}
