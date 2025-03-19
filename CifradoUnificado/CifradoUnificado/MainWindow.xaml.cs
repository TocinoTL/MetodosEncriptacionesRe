using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using AlgoritmoCifrado.Models;
namespace CifradoUnificado
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   	public partial class MainWindow : Window
    {
        private string rutaArchivoSeleccionado;
        private ECDiffieHellmanCng diffieHellman;
        private byte[] publicKey;
        private byte[] sharedKey;
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        Stopwatch stopwatch = new Stopwatch();

        private RSACryptoServiceProvider rsa;
        private Stopwatch RSAstopwatch;
        private PerformanceCounter RSAcpuCounter;
        private string filePath;
        private byte[] sharedKey1;
        private const int Iterations = 1000;
        public MainWindow()
        {
            InitializeComponent();
            rsa = new RSACryptoServiceProvider(2048); // Clave de 2048 bits
            RSAstopwatch = new Stopwatch();
            RSAcpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Abrir cuadro de diálogo para seleccionar archivo
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            openFileDialog.Title = "Seleccionar archivo de texto";

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                FilePathTextBox.Text = filePath; // Mostrar la ruta del archivo en el TextBox
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Por favor, selecciona un archivo de texto primero.");
                return;
            }

            // Leer el archivo de texto
            string inputText;
            try
            {
                inputText = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer el archivo: " + ex.Message);
                return;
            }

            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show("El archivo de entrada está vacío.");
                return;
            }

            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(inputText);
            int blockSize = 214; // Tamaño máximo de bloque para RSA con clave de 2048 bits y padding OAEP
            byte[] encryptedData = null;
            byte[] decryptedData = null;

            // Iniciar mediciones
            stopwatch.Restart();
            long totalMemory = GC.GetTotalMemory(false);
            double cpuUsageStart = cpuCounter.NextValue();

            // Ciclo de 1000 iteraciones
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    // Dividir los datos en bloques y encriptar cada bloque
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        for (int offset = 0; offset < dataToEncrypt.Length; offset += blockSize)
                        {
                            int length = Math.Min(blockSize, dataToEncrypt.Length - offset);
                            byte[] block = new byte[length];
                            Array.Copy(dataToEncrypt, offset, block, 0, length);

                            byte[] encryptedBlock = rsa.Encrypt(block, false);
                            msEncrypt.Write(encryptedBlock, 0, encryptedBlock.Length);
                        }
                        encryptedData = msEncrypt.ToArray();
                    }

                    // Dividir los datos encriptados en bloques y desencriptar cada bloque
                    using (MemoryStream msDecrypt = new MemoryStream())
                    {
                        int encryptedBlockSize = 256; // Tamaño de bloque encriptado (2048 bits = 256 bytes)
                        for (int offset = 0; offset < encryptedData.Length; offset += encryptedBlockSize)
                        {
                            int length = Math.Min(encryptedBlockSize, encryptedData.Length - offset);
                            byte[] block = new byte[length];
                            Array.Copy(encryptedData, offset, block, 0, length);

                            byte[] decryptedBlock = rsa.Decrypt(block, false);
                            msDecrypt.Write(decryptedBlock, 0, decryptedBlock.Length);
                        }
                        decryptedData = msDecrypt.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error durante la encriptación/desencriptación: " + ex.Message);
                    return;
                }
            }

            // Detener mediciones
            stopwatch.Stop();
            long memoryUsed = (GC.GetTotalMemory(false) - totalMemory) / (1024 * 1024); // Memoria en MB
            double cpuUsageEnd = cpuCounter.NextValue();

            // Mostrar resultados
            ResultLabel.Content = "Prueba completada correctamente.";
            TimeLabel.Content = $"Tiempo total: {stopwatch.ElapsedMilliseconds} ms | Tiempo promedio por iteración: {stopwatch.ElapsedMilliseconds / 1000.0:F2} ms";
            MemoryLabel.Content = $"Memoria usada: {memoryUsed} MB";
            CpuLabel.Content = $"Carga del CPU: {cpuUsageEnd:F2} %";
        }




        //-----------------------------------Encriptacion PBE---------------------------------------------------------
        //private void SeleccionarArchivo_Click(object sender, RoutedEventArgs e)
        //{
        //	OpenFileDialog openFileDialog = new OpenFileDialog
        //	{
        //		Title = "Seleccionar archivo",
        //		Filter = "Todos los archivos|*.*|Imágenes|*.png;*.jpg;*.jpeg|Documentos|*.pdf;*.docx"
        //	};

        //	if (openFileDialog.ShowDialog() == true)
        //	{
        //		rutaArchivoSeleccionado = openFileDialog.FileName;
        //		txtRutaArchivo.Text = rutaArchivoSeleccionado;
        //	}
        //}

        // Método para encriptar el archivo seleccionado
        private void EncriptarArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoSeleccionado) || !File.Exists(rutaArchivoSeleccionado))
            {
                MessageBox.Show("Seleccione un archivo válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string password = txtPassword.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingrese una contraseña para la encriptación.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int iteraciones;
            if (string.IsNullOrWhiteSpace(txtIteraciones.Text) || !int.TryParse(txtIteraciones.Text, out iteraciones) || iteraciones <= 0)
            {
                MessageBox.Show("Ingrese un número válido de iteraciones mayor a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            long memoriaAntes = GetMemoryUsage();
            // Crear el PerformanceCounter para la carga de la CPU
            cpuCounter.NextValue(); // Inicializamos el contador


            try
            {

                byte[] fileContent = File.ReadAllBytes(rutaArchivoSeleccionado);

                // Generar la clave y vector de inicialización (IV) usando PBE
                stopwatch.Start();
                using (var aes = new AesManaged())
                {
                    byte[] salt = GenerateSalt();
                    Rfc2898DeriveBytes keyGen = new Rfc2898DeriveBytes(password, salt, iteraciones);
                    aes.Key = keyGen.GetBytes(32); // AES-256
                    aes.IV = keyGen.GetBytes(16);  // IV de 16 bytes

                    // Encriptar el contenido
                    byte[] encryptedData = EncryptData(fileContent, aes.Key, aes.IV);

                    // Guardar el archivo encriptado
                    string encryptedFilePath = Path.ChangeExtension(rutaArchivoSeleccionado, ".txt");

                    // Verificar si el archivo ya existe y, si es así, eliminarlo
                    //if (File.Exists(encryptedFilePath))
                    //{
                    //	File.Delete(encryptedFilePath);
                    //}

                    using (FileStream fs = new FileStream(encryptedFilePath, FileMode.Create))
                    {
                        fs.Write(salt, 0, salt.Length); // Guardamos el salt al inicio
                        fs.Write(aes.IV, 0, aes.IV.Length); // Guardamos el IV
                        fs.Write(encryptedData, 0, encryptedData.Length); // Guardamos el contenido encriptado
                    }
                    stopwatch.Stop();


                    long tiempoTotal = stopwatch.ElapsedMilliseconds;
                    double tiempoPromedioPorIteracion = (double)tiempoTotal / iteraciones;
                    // Obtener el uso de memoria después de la operación
                    long memoriaDespues = GetMemoryUsage();
                    // Calcular el uso de memoria
                    long memoriaUtilizada = memoriaDespues - memoriaAntes;
                    // Medir y mostrar la carga de la CPU durante el proceso
                    float cpuUsage = cpuCounter.NextValue();

                    resultsData(encryptedFilePath, tiempoTotal, tiempoPromedioPorIteracion, memoriaUtilizada, cpuUsage, "Encriptar");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al encriptar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Método para obtener el uso de memoria en bytes
        private long GetMemoryUsage()
        {
            Process currentProcess = Process.GetCurrentProcess();
            return currentProcess.WorkingSet64; // Memoria en uso (en bytes)
        }
        // Método para generar un salt aleatorio
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        // Método para encriptar los datos usando AES
        private static byte[] EncryptData(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }










        //*****************************Algoritmo PBE***********************************************************************************************************************


        //----------------------------------------------------------------------------------------------------
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("SaltFijo1234");

        private async void btnProcesar_Click(object sender, RoutedEventArgs e)
        {
            Data data = new Data();
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
            long memoriaAntes = GetMemoryUsage();


            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string content = File.ReadAllText(filePath);
                int iteraciones = int.Parse(txtIteraciones.Text);
                string password = txtPassword.Password;
                btnProcesar.IsEnabled = false;

                // Ejecuta en un hilo en segundo plano
                await Task.Run(() =>
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    string result = content;
                    for (int i = 0; i < iteraciones; i++)
                    {
                        result = Encrypt(result, password);
                        result = Decrypt(result, password);
                    }

                    stopwatch.Stop();
                    long tiempoTotal = stopwatch.ElapsedMilliseconds;
                    double tiempoPromedio = (double)tiempoTotal / iteraciones;
                    long memoriaDespues = GetMemoryUsage();
                    long memoriaUtilizada = memoriaDespues - memoriaAntes;
                    float cpuUsage = cpuCounter.NextValue();



                    // Guardar archivo en el hilo principal
                    Dispatcher.Invoke(() =>
                    {
                        string finalPath = filePath.Replace(".txt", "_final.txt");
                        File.WriteAllText(finalPath, result);
                        MessageBox.Show($"Tiempo total: {tiempoTotal}ms");
                        btnProcesar.IsEnabled = true;
                        data = new Data(finalPath, tiempoTotal, tiempoPromedio, memoriaUtilizada, cpuUsage);
                        Result(data);
                    });
                });
            }
        }
        // 🔥 Método de Encriptación usando PBE + XOR
        private static string Encrypt(string text, string password)
        {
            byte[] key = GenerateKey(password);
            byte[] data = Encoding.UTF8.GetBytes(text);

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length]; // XOR con la clave generada
            }

            return Convert.ToBase64String(data);
        }

        // 🔥 Método de Desencriptación usando PBE + XOR
        private static string Decrypt(string encryptedText, string password)
        {
            byte[] key = GenerateKey(password);
            byte[] data = Convert.FromBase64String(encryptedText);

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length]; // XOR con la clave generada (reversible)
            }

            return Encoding.UTF8.GetString(data);
        }

        // 🔥 Generar clave PBE con Rfc2898DeriveBytes
        private static byte[] GenerateKey(string password)
        {
            using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, Salt, 10000))
            {
                return pdb.GetBytes(32); // Clave de 32 bytes
            }
        }


        public void Result(Data data)
        {
            ttbResultPBE.Text = $"Archivo guardado en: {data.Filename}\n" +
                                  $"Tiempo Total: {data.Time} ms\n" +
                                  $"Promedio por iteración: {data.TimePromedio} ms\n" +
                                  $"Memoria utilizada: {data.TimePromedio} bytes\n" +
                                  $"Uso de CPU: {data.cpuUsage}%\n";
        }

        //----------------------------------------------------------------------------------------------------




        //*****************************Algoritmo PBE***********************************************************************************************************************









        //------------------------------------------------------------------------------------------------------------------------
        // Método para desencriptar los datos
        private void btnDesencriptar_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoSeleccionado) || !File.Exists(rutaArchivoSeleccionado))
            {
                MessageBox.Show("Seleccione un archivo válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string password = txtPassword.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingrese la contraseña para desencriptar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int iteraciones = int.Parse(txtIteraciones.Text); // Obtener las iteraciones desde la UI

            // Crear el objeto Stopwatch para medir el tiempo de ejecución

            // Obtener el uso de memoria antes de la operación
            long memoriaAntes = GetMemoryUsage();
            // Crear el PerformanceCounter para la carga de la CPU

            cpuCounter.NextValue(); // Inicializamos el contador

            try
            {
                byte[] fileContent = File.ReadAllBytes(rutaArchivoSeleccionado);

                // Leer el salt, IV y el contenido cifrado
                byte[] salt = new byte[16];
                byte[] iv = new byte[16];
                byte[] encryptedData = new byte[fileContent.Length - 32];

                Buffer.BlockCopy(fileContent, 0, salt, 0, 16);
                Buffer.BlockCopy(fileContent, 16, iv, 0, 16);
                Buffer.BlockCopy(fileContent, 32, encryptedData, 0, encryptedData.Length);



                using (var aes = new AesManaged())
                {
                    Rfc2898DeriveBytes keyGen = new Rfc2898DeriveBytes(password, salt, iteraciones);
                    aes.Key = keyGen.GetBytes(32);
                    aes.IV = iv;

                    // Desencriptar los datos
                    byte[] decryptedData = DecryptData(encryptedData, aes.Key, aes.IV);

                    // Guardar el archivo desencriptado
                    string decryptedFilePath = rutaArchivoSeleccionado.Replace(".enc", ".dec");
                    File.WriteAllBytes(decryptedFilePath, decryptedData);

                    MessageBox.Show($"Archivo desencriptado y guardado en:\n{decryptedFilePath}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                stopwatch.Stop();

                //Tiempo de Ejecucion
                long tiempoTotal = stopwatch.ElapsedMilliseconds;
                double tiempoPromedioPorIteracion = (double)tiempoTotal / iteraciones;

                // Obtener el uso de memoria después de la operación
                long memoriaDespues = GetMemoryUsage();
                // Calcular el uso de memoria
                long memoriaUtilizada = memoriaDespues - memoriaAntes;
                // Medir y mostrar la carga de la CPU durante el proceso
                float cpuUsage = cpuCounter.NextValue();

                resultsData(rutaArchivoSeleccionado, tiempoTotal, tiempoPromedioPorIteracion, memoriaUtilizada, cpuUsage, "Desencriptar");


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desencriptar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private static byte[] DecryptData(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return decryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }
        private void resultsData(string encryptedFilePath, long tiempoTotal, double tiempoPromedioPorIteracion, long memoriaUtilizada, float cpuUsage, string Type)
        {
            ttbResultPBE.Text = "";
            // Mostrar el resultado
            MessageBox.Show($"Archivo guardado en:\n{encryptedFilePath}\n" +
                             $"Tiempo total: {tiempoTotal / 1000.0} seg - {tiempoTotal} ms\n" +
                             $"Promedio por iteración: {tiempoPromedioPorIteracion:F3} ms/iteración\n" +
                             $"Memoria utilizada: {memoriaUtilizada / 1024} KB \n" +
                             $"Carga de la CPU: {cpuUsage}%",
                             $"{Type}", MessageBoxButton.OK, MessageBoxImage.Information);



            ttbResultPBE.Text = $"Operación: {Type}\n" +
              $"Archivo guardado en: {encryptedFilePath}\n" +
              $"Tiempo Total: {tiempoTotal} ms\n" +
              $"Promedio por iteración: {tiempoPromedioPorIteracion} ms\n" +
              $"Memoria utilizada: {memoriaUtilizada} bytes\n" +
              $"Uso de CPU: {cpuUsage}%\n";
        }
        //-----------------------------------------------------------------------------------------------------------------------
















        //---------------------------------------------------------sha
        private void resultsDataSHA(string encryptedFilePath, long tiempoTotal, double tiempoPromedioPorIteracion, long memoriaUtilizada, float cpuUsage, string Type)
        {
            txtHashSHA.Text = "";
            // Mostrar el resultado
            MessageBox.Show($"Archivo guardado en:\n{encryptedFilePath}\n" +
                             $"Tiempo total: {tiempoTotal / 1000.0} seg - {tiempoTotal} ms\n" +
                             $"Promedio por iteración: {tiempoPromedioPorIteracion:F3} ms/iteración\n" +
                             $"Memoria utilizada: {memoriaUtilizada / 1024} KB \n" +
                             $"Carga de la CPU: {cpuUsage}%",
                             $"{Type}", MessageBoxButton.OK, MessageBoxImage.Information);



            txtHashSHA.Text = $"Operación: {Type}\n" +
              $"Archivo guardado en: {encryptedFilePath}\n" +
              $"Tiempo Total: {tiempoTotal} ms\n" +
              $"Promedio por iteración: {tiempoPromedioPorIteracion} ms\n" +
              $"Memoria utilizada: {memoriaUtilizada} bytes\n" +
              $"Uso de CPU: {cpuUsage}%\n";
        }



        private string rutaArchivoSHA;
        private void SeleccionarArchivoSHA_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo de texto",
                Filter = "Archivos de texto|*.txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                rutaArchivoSHA = openFileDialog.FileName;
                txtRutaArchivoSHA.Text = rutaArchivoSHA;

                // Leer el contenido del archivo
                try
                {
                    string contenido = File.ReadAllText(rutaArchivoSHA);
                    txtContenidoArchivoSHA.Text = contenido;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EncriptarSHA_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(rutaArchivoSHA) || !File.Exists(rutaArchivoSHA))
            {
                MessageBox.Show("Seleccione un archivo válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtener el contenido del archivo
            string contenido = txtContenidoArchivoSHA.Text;
            if (string.IsNullOrWhiteSpace(contenido))
            {
                MessageBox.Show("El archivo está vacío o no tiene contenido válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Asignación del valor de iteraciones
            int iteraciones = 0;
            bool esNumeroValido = int.TryParse(txtIteracionesSHA.Text, out iteraciones);

            // Validar si el número es válido y mayor a 0
            if (!esNumeroValido || iteraciones <= 0)
            {
                MessageBox.Show("Ingrese un número válido de iteraciones mayor a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Medir memoria antes del proceso
            long memoriaAntes = GetMemoryUsage();
            cpuCounter.NextValue();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Aplicar SHA-256 repetidamente
            string hashSHA256 = await Task.Run(() => GenerarSHA256Multiple(contenido, iteraciones));

            stopwatch.Stop();

            // Medir memoria después del proceso
            long memoriaDespues = GetMemoryUsage();
            long memoriaUtilizada = memoriaDespues - memoriaAntes;
            float cpuUsage = cpuCounter.NextValue();
            long tiempoTotal = stopwatch.ElapsedMilliseconds;
            double tiempoPromedioPorIteracion = (double)tiempoTotal / iteraciones;

            // Mostrar el hash en la interfaz
            txtHashSHA.Text = hashSHA256;

            // Guardar el hash en un archivo
            string hashedFilePath = rutaArchivoSHA + ".sha256";
            try
            {
                File.WriteAllText(hashedFilePath, hashSHA256);
                resultsDataSHA(hashedFilePath, tiempoTotal, tiempoPromedioPorIteracion, memoriaUtilizada, cpuUsage, "SHA-256");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el hash: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método para generar el hash SHA-256 múltiples veces
        private static string GenerarSHA256Multiple(string input, int iteraciones)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                for (int i = 0; i < iteraciones; i++)
                {
                    inputBytes = sha256.ComputeHash(inputBytes);
                }

                // Convertir los bytes a una cadena hexadecimal
                StringBuilder sb = new StringBuilder();
                foreach (byte b in inputBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }






















































        //--------------------------------------------AES-------------------
        private string ruta_archivo;
        private void SeleccionarArchivoAESClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo",
                Filter = "Todos los archivos|*.*|Imágenes|*.png;*.jpg;*.jpeg|Documentos|*.pdf;*.docx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ruta_archivo = openFileDialog.FileName;
                txtRutaArchivoAES.Text = ruta_archivo;

                // Limpiar campos al seleccionar un nuevo archivo
                txtPasswordAES.Password = string.Empty;
                Cant_Iteraciones.Text = string.Empty;
                txtResultadoAES.Text = string.Empty;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------


        // -------------------------------------Encriptar archivo----------------------------------------


        private async void Btn_EncriptarAES_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ruta_archivo))
            {
                MessageBox.Show("Debe seleccionar un archivo antes de continuar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtPasswordAES.Password))
            {
                MessageBox.Show("Debe ingresar una contraseña para encriptar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(Cant_Iteraciones.Text, out int iteraciones) || iteraciones <= 0)
            {
                MessageBox.Show("Ingrese un número válido de iteraciones (mayor a 0).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string password = txtPasswordAES.Password;
            byte[] aesKey = GetAESKey(password, iteraciones);
            if (aesKey == null)
            {
                MessageBox.Show("La clave debe tener 16, 24 o 32 caracteres para AES-128, AES-192 o AES-256.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string originalText = File.ReadAllText(ruta_archivo);
            txtResultadoAES.Text = "Procesando...";

            await Task.Run(() => ProcesarAES(originalText, aesKey, iteraciones));

            // Después de encriptar, desencriptar el texto y mostrar el resultado
            string encryptedText = EncryptAES(originalText, aesKey);
            string decryptedText = DecryptAES(encryptedText, aesKey); // Llamamos al método de desencriptación


            /*Dispatcher.Invoke(() =>
            {
                // Mostrar el texto desencriptado en el resultado
                txtResultadoAES.Text = $"Texto Desencriptado:\n{decryptedText}";
            });*/
        }


        private void ProcesarAES(string originalText, byte[] aesKey, int iteraciones)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            long initialMemory = GC.GetTotalMemory(true);
            double initialCpuTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;

            for (int i = 0; i < iteraciones; i++)
            {
                string encrypted = EncryptAES(originalText, aesKey);
                _ = DecryptAES(encrypted, aesKey); // Se desencripta solo para prueba
            }

            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(true);
            double finalCpuTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;

            long memoryUsed = finalMemory - initialMemory;
            double cpuUsage = (finalCpuTime - initialCpuTime) / iteraciones;
            double avgTimePerIteration = stopwatch.ElapsedMilliseconds / (double)iteraciones;

            Dispatcher.Invoke(() =>
            {
                txtResultadoAES.Text = $"Proceso completado.\n" +
                                       $"Tiempo Total: {stopwatch.ElapsedMilliseconds} ms\n" +
                                       $"Promedio por Iteración: {avgTimePerIteration:F2} ms\n" +
                                       $"Memoria Usada: {memoryUsed / 1024} KB\n" +
                                       $"Uso de CPU: {cpuUsage:F2} ms por iteración";
            });
        }

        // Método para derivar una clave AES utilizando PBKDF2 y un salt aleatorio
        private byte[] GetAESKey(string password, int iterations)
        {
            // Crear un salt aleatorio
            byte[] salt = new byte[16]; // Tamaño común para un salt
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);  // Llenar el salt con valores aleatorios
            }

            // Derivar la clave utilizando PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {

                byte[] key = pbkdf2.GetBytes(32); // Para AES-256, usa 32 bytes
                return key;
            }
        }
        //--------------------------------------METODO PARA LA ENCRIPTACION----AES---------------------
        private string EncryptAES(string plainText, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = new byte[16]; // Usar un IV estático o aleatorio
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }


        //----------------------------DESSENCRIPTAR AES-------------------------


        private string DecryptAES(string cipherText, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = new byte[16]; // Usar un IV estático o aleatorio
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }


        //-------------------------------------------------------------------------------------------------------------------
        //modificacion11

        private void BtnEncriptar_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtAnio.Text, out int anio))
            {
                string palabra = txtPalabra.Text;
                string encriptado = Encriptar(palabra, anio);
                txtResultadoM.Text = encriptado;
            }
            else
            {
                MessageBox.Show("Ingrese un año válido.");
            }
        }

        private void BtnDesencriptar_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtAnio.Text, out int anio))
            {
                string[] numeros = txtResultadoM.Text.Split(',');
                string desencriptado = Desencriptar(numeros, anio);
                txtResultadoM.Text = desencriptado;
            }
            else
            {
                MessageBox.Show("Ingrese un año válido.");
            }
        }

        private string Encriptar(string palabra, int anio)
        {
            return string.Join(",", palabra.Select(c => ((int)c - anio).ToString()));
        }

        private string Desencriptar(string[] numeros, int anio)
        {
            return new string(numeros.Select(n => (char)(int.Parse(n) + anio)).ToArray());
        }

        private void txtClavePublica_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt",
                Title = "Seleccione un archivo de texto"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                FileNameTextBlock.Text = $"Archivo: {System.IO.Path.GetFileName(filePath)}";

                try
                {
                    // Leer el contenido del archivo y mostrarlo en el TextBox
                    InputTextBox.Text = File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al leer el archivo: {ex.Message}");
                }
            }
        }

        private void StartEncryption(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                MessageBox.Show("Por favor, seleccione un archivo o ingrese texto para encriptar.");
                return;
            }

            RunDiffieHellman();
            MeasurePerformance();
        }

        private void RunDiffieHellman()
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            using (ECDiffieHellmanCng bob = new ECDiffieHellmanCng())
            {
                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;

                bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                bob.HashAlgorithm = CngAlgorithm.Sha256;

                sharedKey = alice.DeriveKeyMaterial(bob.PublicKey);
            }
        }

        private void MeasurePerformance()
        {
            string text = InputTextBox.Text;
            byte[] encryptedData = null;
            byte[] decryptedData = null;
            Stopwatch stopwatch = new Stopwatch();

            // Medir CPU
            Process currentProcess = Process.GetCurrentProcess();
            TimeSpan startCpuTime = currentProcess.TotalProcessorTime;
            stopwatch.Start();

            for (int i = 0; i < Iterations; i++)
            {
                encryptedData = Encrypt(text);
                decryptedData = Decrypt(encryptedData);
            }

            stopwatch.Stop();
            TimeSpan endCpuTime = currentProcess.TotalProcessorTime;

            // Calcular uso de CPU
            double cpuUsage = (endCpuTime - startCpuTime).TotalMilliseconds / stopwatch.ElapsedMilliseconds * 100;

            // Calcular uso de memoria
            long memoryUsage = GC.GetTotalMemory(true);
            double memoryUsageKB = memoryUsage / 1024.0; // Convertir a KB

            // Calcular tiempo promedio por iteración
            double avgTime = stopwatch.ElapsedMilliseconds / (double)Iterations;

            ResultTextBlock.Text = $"Tiempo total: {stopwatch.ElapsedMilliseconds} ms\n"
                + $"Tiempo promedio por iteración: {avgTime:F2} ms\n"
                + $"Memoria utilizada: {memoryUsageKB:F2} KB\n"
                + $"Uso de CPU: {cpuUsage:F6}%";
        }

        private byte[] Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = sharedKey;
                aes.GenerateIV();
                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                    // Concatenar IV al inicio del mensaje cifrado
                    byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                    Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                    Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                    return result;
                }
            }
        }

        private byte[] Decrypt(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = sharedKey;

                // Extraer IV del mensaje cifrado
                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] encryptedText = new byte[encryptedData.Length - iv.Length];

                Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(encryptedData, iv.Length, encryptedText, 0, encryptedText.Length);

                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return decryptor.TransformFinalBlock(encryptedText, 0, encryptedText.Length);
                }
            }
        }
    }
}