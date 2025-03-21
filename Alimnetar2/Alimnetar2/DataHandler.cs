using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alimnetar2
{
    class DataHandler
    {
        private string connectionString = @"Server=COMTORRAX\SQLEXPRESS;Database=ControlAsistencia;User Id=sa;Password=qwerty;";

        public int InsertEmpleado(string nombre, string departamento)
        {
            // Consulta para:
            // 1) Buscar si ya existe un empleado con el mismo Nombre
            // 2) Si no existe, insertar nuevo
            // 3) Devolver el ID (nuevo o existente)
                    string query = @"
                                        DECLARE @ExistingId INT;

                                        SELECT @ExistingId = ID
                                        FROM Empleados
                                        WHERE Nombre = @Nombre;

                                        IF @ExistingId IS NULL
                                        BEGIN
                                            INSERT INTO Empleados (Nombre, Departamento)
                                            VALUES (@Nombre, @Departamento);

                                            -- Obtener el ID recién insertado
                                            SET @ExistingId = SCOPE_IDENTITY();
                                        END

                                        SELECT @ExistingId;
                                    ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nombre", nombre);
                command.Parameters.AddWithValue("@Departamento", departamento);

                connection.Open();

                // ExecuteScalar() devuelve el valor de la primera columna
                // de la primera fila del resultado (en este caso, @ExistingId)
                object result = command.ExecuteScalar();

                // Convertir a int
                return Convert.ToInt32(result);
            }
        }

        //public int InsertRegistroAsistencia(int empleadoID, DateTime fechaHora, string tipo)
        //{
        //    string query = "INSERT INTO RegistrosAsistencia (EmpleadoID, FechaHora, Tipo) VALUES (@EmpleadoID, @FechaHora, @Tipo)";

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    using (SqlCommand command = new SqlCommand(query, connection))
        //    {
        //        command.Parameters.AddWithValue("@EmpleadoID", empleadoID);
        //        command.Parameters.AddWithValue("@FechaHora", fechaHora);
        //        command.Parameters.AddWithValue("@Tipo", tipo);

        //        connection.Open();
        //        return command.ExecuteNonQuery();
        //    }
        //}
        // Nueva versión del método
        //public int InsertRegistroAsistencia(int empleadoID, DateTime fecha, TimeSpan hora, string tipo)
        //{
        //    string query = @"
        //                INSERT INTO RegistrosAsistencia (EmpleadoID, Fecha, Hora, Tipo)
        //                VALUES (@EmpleadoID, @Fecha, @Hora, @Tipo);
        //            ";

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    using (SqlCommand command = new SqlCommand(query, connection))
        //    {
        //        command.Parameters.AddWithValue("@EmpleadoID", empleadoID);
        //        command.Parameters.AddWithValue("@Fecha", fecha);         // date
        //        command.Parameters.AddWithValue("@Hora", hora);           // time
        //        command.Parameters.AddWithValue("@Tipo", tipo);

        //        connection.Open();
        //        return command.ExecuteNonQuery();
        //    }
        //}
        public int InsertRegistroAsistencia(int empleadoID, DateTime fecha, string horaEncriptada, string tipo)
        {
            string query = @"
        INSERT INTO RegistrosAsistencia (EmpleadoID, Fecha, Hora, Tipo)
        VALUES (@EmpleadoID, @Fecha, @Hora, @Tipo);";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmpleadoID", empleadoID);
                command.Parameters.AddWithValue("@Fecha", fecha);           // date
                command.Parameters.AddWithValue("@Hora", horaEncriptada);   // nvarchar
                command.Parameters.AddWithValue("@Tipo", tipo);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }


        public int InsertHorasTrabajadas(int empleadoID, string tiempoTotal, string horasTotales)
        {
            string query = "INSERT INTO HorasTrabajadas (EmpleadoID, TiempoTotal, HorasTotales) VALUES (@EmpleadoID, @TiempoTotal, @HorasTotales)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmpleadoID", empleadoID);
                command.Parameters.AddWithValue("@TiempoTotal", tiempoTotal);
                command.Parameters.AddWithValue("@HorasTotales", horasTotales);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int InsertTotalesAsistencia(int empleadoID, string totalEntrada, string totalSalida)
        {
            string query = "INSERT INTO TotalesAsistencia (EmpleadoID, TotalEntrada, TotalSalida) VALUES (@EmpleadoID, @TotalEntrada, @TotalSalida)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmpleadoID", empleadoID);
                command.Parameters.AddWithValue("@TotalEntrada", totalEntrada);
                command.Parameters.AddWithValue("@TotalSalida", totalSalida);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}
