using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace WinRARCompression
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        const uint OPEN_EXISTING = 3;
        const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;

        static void Main(string[] args)
        {
            string driveLetter = "F"; // Cambia esta letra por la letra de la unidad USB que quieras expulsar
            string drivePath = "\\\\.\\" + driveLetter + ":";

            string rarExePath = @"C:\Program Files\WinRAR\WinRAR.exe"; // Ruta al ejecutable de WinRAR
            string sourceFolderPath = @"\\172.16.0.3\F$\BACKUPDVD";
            string rarFilePath = @"F:\comprimido.rar";

            // Acceder a la carpeta "Prueba" en el servidor
            DirectoryInfo directorioPrueba = new DirectoryInfo(sourceFolderPath);

            // Obtener los subdirectorios dentro de la carpeta "Prueba"
            var subdirectorios = directorioPrueba.GetDirectories();

            if (subdirectorios.Length > 0)
            {
                // Seleccionar el subdirectorio más reciente
                DirectoryInfo carpetaReciente = subdirectorios.OrderByDescending(d => d.LastWriteTimeUtc).First();

                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = rarExePath;
                    startInfo.Arguments = $"a -r -ep1 \"{rarFilePath}\" \"{carpetaReciente.FullName}\\*\""; // Comprimir el contenido del subdirectorio más reciente y eliminar el componente de ruta base

                    using (Process process = new Process())
                    {
                        process.StartInfo = startInfo;
                        process.Start();
                        process.WaitForExit();

                        Console.WriteLine("Carpeta comprimida con éxito.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al comprimir la carpeta: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No se encontraron subdirectorios en la carpeta 'Prueba'.");
            }

            // Código para expulsar la unidad USB)

            IntPtr handle = CreateFile(
                drivePath,
                GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (handle.ToInt32() != -1)
            {
                // Espera cinco segundo antes de continuar
                System.Threading.Thread.Sleep(50000);

                uint dummy = 0;
                DeviceIoControl(
                    handle,
                    IOCTL_STORAGE_EJECT_MEDIA,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    0,
                    ref dummy,
                    IntPtr.Zero);

                CloseHandle(handle);

                Console.WriteLine("Unidad USB expulsada exitosamente.");
            }
            else
            {
                Console.WriteLine("No se pudo abrir la unidad USB.");
            }

            Console.WriteLine("Presione cualquier tecla para salir.");
            Console.ReadKey();
        }

       
        }
    }










