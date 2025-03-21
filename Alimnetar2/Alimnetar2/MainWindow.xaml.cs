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
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Globalization;


namespace Alimnetar2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataHandler dataHandler = new DataHandler();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSeleccionarArchivo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ProcesarArchivo(openFileDialog.FileName);
            }
        }

        private void ProcesarArchivo(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                List<string[]> datosProcesados = new List<string[]>();

                if (lines.Length < 3)
                {
                    MessageBox.Show("El archivo no tiene suficientes líneas para procesar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                for (int i = 2; i < lines.Length - 1; i++) // Ignorar primera, segunda y última línea
                {
                    string[] valores = lines[i].Split(',');

                    if (valores.Length < 10) // Validar cantidad mínima de columnas
                    {
                        MessageBox.Show($"Error en la línea {i + 1}: datos insuficientes.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    datosProcesados.Add(valores);
                }

                GuardarDatosEnSQL(datosProcesados);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void GuardarDatosEnSQL(List<string[]> datos)
        //{
        //    try
        //    {
        //        int empleadoID = 0;

        //        foreach (var fila in datos)
        //        {
        //            try
        //            {
        //                if (fila[0].Trim() == "ID")
        //                {
        //                    empleadoID = int.Parse(fila[1]);
        //                    string nombre = fila[4].Trim();
        //                    string departamento = fila[8].Trim();

        //                    if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(departamento))
        //                    {
        //                        dataHandler.InsertEmpleado(empleadoID, nombre, departamento);
        //                    }
        //                }
        //                else if (fila[0].Trim().Contains("/")) // Procesar registros de asistencia
        //                {
        //                    if (empleadoID == 0) continue;

        //                    int i = 0;
        //                    while (i < fila.Length)
        //                    {
        //                        string valor = fila[i].Trim();

        //                        // Verificamos si valor es una fecha/hora
        //                        if (DateTime.TryParseExact(
        //                            valor,
        //                            "dd/MM/yyyy HH:mm",
        //                            CultureInfo.InvariantCulture,
        //                            DateTimeStyles.None,
        //                            out DateTime fechaHora))
        //                        {
        //                            // Valor por defecto
        //                            string tipo = "Entrada";

        //                            // 3. Miramos la siguiente columna para ver si es "Entrada", "Salida" o está vacía
        //                            if (i + 1 < fila.Length)
        //                            {
        //                                string posibleTipo = fila[i + 1].Trim();

        //                                if (posibleTipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase)
        //                                    || posibleTipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    // Asignamos tipo y avanzamos un paso más
        //                                    tipo = posibleTipo;
        //                                    i++;
        //                                }
        //                                else if (string.IsNullOrEmpty(posibleTipo))
        //                                {
        //                                    // 4. Si está vacía, miramos una segunda vez (i+2)
        //                                    if (i + 2 < fila.Length)
        //                                    {
        //                                        string posibleTipo2 = fila[i + 2].Trim();
        //                                        if (posibleTipo2.Equals("Entrada", StringComparison.OrdinalIgnoreCase)
        //                                            || posibleTipo2.Equals("Salida", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            tipo = posibleTipo2;
        //                                            // Nos saltamos ambas columnas vacías
        //                                            i += 2;
        //                                        }
        //                                        // Si también está vacía o no coincide, nos quedamos con "Entrada" por defecto
        //                                    }
        //                                }
        //                            }

        //                            // Insertamos el registro
        //                            dataHandler.InsertRegistroAsistencia(empleadoID, fechaHora, tipo);
        //                        }
        //                        // Avanzamos de cualquier modo
        //                        i++;
        //                    }
        //                }
        //                else if (fila[0].Trim() == "Entrada")
        //                {
        //                    if (empleadoID == 0) continue; // Asegurar que hay un empleado asignado

        //                    string totalEntradas = fila[1].Trim();
        //                    string totalSalidas = fila[4].Trim();

        //                    // Extraer correctamente "Tiempo Total"
        //                    //string tiempoTotal = fila[Array.IndexOf(fila, "Tiempo total") + 1].Trim();

        //                    //// Extraer correctamente "Horas Totales"
        //                    //string horasTotales = fila[Array.IndexOf(fila, "Horas totales") + 1].Trim().Replace(",", ".");
        //                    string tiempoTotal = fila[6].Trim();

        //                    // Extraer correctamente "Horas Totales"
        //                    string horasTotales = fila[9].Trim() + ";" + fila[10].Trim();

        //                    dataHandler.InsertHorasTrabajadas(empleadoID, tiempoTotal, horasTotales);
        //                    dataHandler.InsertTotalesAsistencia(empleadoID, totalEntradas, totalSalidas);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Error en la línea: {string.Join(",", fila)}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }

        //        MessageBox.Show("Datos importados con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error al guardar los datos en la base: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void GuardarDatosEnSQL(List<string[]> datos)
        {
            try
            {
                // Definimos la variable para guardar el ID
                int empleadoID = 0;

                foreach (var fila in datos)
                {
                    try
                    {
                        // Cuando la fila tenga la marca "ID" (según tu formato)
                        if (fila[0].Trim() == "ID")
                        {
                            // En lugar de parsear fila[1] como ID,
                            // usas directamente Nombre y Departamento de la línea
                            string nombre = fila[4].Trim();
                            string departamento = fila[8].Trim();

                            if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(departamento))
                            {
                                // dataHandler.InsertEmpleado(...) te devuelve el ID de BD
                                empleadoID = dataHandler.InsertEmpleado(nombre, departamento);

                                // A partir de aquí, empleadoID es el verdadero ID en la BD
                            }
                        }
                        else if (fila[0].Trim().Contains("/")) // Procesar registros de asistencia
                        {
                            // Asegurar que ya obtuviste un empleadoID válido
                            if (empleadoID == 0) continue;

                            int i = 0;
                            //while (i < fila.Length)
                            //{
                            //    string valor = fila[i].Trim();

                            //    // Verificamos si valor es una fecha/hora
                            //    if (DateTime.TryParseExact(
                            //        valor,
                            //        "dd/MM/yyyy HH:mm",
                            //        CultureInfo.InvariantCulture,
                            //        DateTimeStyles.None,
                            //        out DateTime fechaHora))
                            //    {
                            //        // Valor por defecto
                            //        string tipo = "Entrada";

                            //        // Revisar siguiente columna para ver si "Entrada" o "Salida"
                            //        if (i + 1 < fila.Length)
                            //        {
                            //            string posibleTipo = fila[i + 1].Trim();
                            //            if (posibleTipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase) ||
                            //                posibleTipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
                            //            {
                            //                tipo = posibleTipo;
                            //                i++;
                            //            }
                            //            else if (string.IsNullOrEmpty(posibleTipo))
                            //            {
                            //                // Revisar dos columnas adelante
                            //                if (i + 2 < fila.Length)
                            //                {
                            //                    string posibleTipo2 = fila[i + 2].Trim();
                            //                    if (posibleTipo2.Equals("Entrada", StringComparison.OrdinalIgnoreCase) ||
                            //                        posibleTipo2.Equals("Salida", StringComparison.OrdinalIgnoreCase))
                            //                    {
                            //                        tipo = posibleTipo2;
                            //                        i += 2;
                            //                    }
                            //                }
                            //            }
                            //        }

                            //        // Insertamos el registro de asistencia con el ID de la BD
                            //        dataHandler.InsertRegistroAsistencia(empleadoID, fechaHora, tipo);
                            //    }

                            //    i++;
                            //}
                            while (i < fila.Length)
                            {
                                string valor = fila[i].Trim();
                                // Verificamos si valor es una fecha/hora
                                if (DateTime.TryParseExact(
                                            valor,
                                            "dd/MM/yyyy HH:mm",
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.None,
                                            out DateTime fechaHora))
                                {
                                    string tipo = "Entrada";
                                    // (Ya tienes la lógica para determinar "Entrada"/"Salida")
                                    if (i + 1 < fila.Length)
                                    {
                                        string posibleTipo = fila[i + 1].Trim();
                                        if (posibleTipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase) ||
                                            posibleTipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
                                        {
                                            tipo = posibleTipo;
                                            i++;
                                        }
                                        else if (string.IsNullOrEmpty(posibleTipo))
                                        {
                                            // Revisar dos columnas adelante
                                            if (i + 2 < fila.Length)
                                            {
                                                string posibleTipo2 = fila[i + 2].Trim();
                                                if (posibleTipo2.Equals("Entrada", StringComparison.OrdinalIgnoreCase) ||
                                                    posibleTipo2.Equals("Salida", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    tipo = posibleTipo2;
                                                    i += 2;
                                                }
                                            }
                                        }
                                    }
                                    var soloFecha = fechaHora.Date;
                                    var soloHora = fechaHora.TimeOfDay;

                                    // Convertir TimeSpan a string (formato "HH:mm:ss" o como desees)
                                    string horaString = soloHora.ToString(@"hh\:mm\:ss");
                                    // Encriptar la hora
                                    string horaEncriptada = CryptoHelper.EncryptAES(horaString, "qwerty");

                                    // Llamar a tu método para guardar
                                    dataHandler.InsertRegistroAsistencia(empleadoID, soloFecha, horaEncriptada, tipo);
                                }
                                i++;
                            }
                        }
                        else if (fila[0].Trim() == "Entrada")
                        {
                            // Asegurar que hay un empleado asignado
                            if (empleadoID == 0) continue;

                            string totalEntradas = fila[1].Trim();
                            string totalSalidas = fila[4].Trim();
                            string tiempoTotal = fila[6].Trim();

                            // Ejemplo: concatenas índices 9 y 10
                            string horasTotales = fila[9].Trim() + ":" + fila[10].Trim();

                            // Insertar horas, totales, etc.
                            dataHandler.InsertHorasTrabajadas(empleadoID, tiempoTotal, horasTotales);
                            dataHandler.InsertTotalesAsistencia(empleadoID, totalEntradas, totalSalidas);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error en la línea: {string.Join(",", fila)}\n{ex.Message}",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }

                MessageBox.Show("Datos importados con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar los datos en la base: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

    }
}
