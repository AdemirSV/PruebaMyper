using Microsoft.AspNetCore.Mvc;
using PruebaMyper.Models;
using System.Data.SqlClient;

namespace PruebaMyper.Controllers
{
    public class TrabajadorController : Controller
    {
        private readonly IConfiguration _configuration;
        public TrabajadorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private SqlConnection Conexion()
        {
            string connectionString = _configuration.GetConnectionString("cadena");
            return new SqlConnection(connectionString);
        }
        public IActionResult Index(string sexo)
        {
            List<Trabajador> lista = new List<Trabajador>();
            using (SqlConnection conn = Conexion())
            {
                conn.Open();
                string sql =@"SELECT 
                            t.Id,
                            t.TipoDocumento,
                            t.NumeroDocumento,
                            t.Nombres,
                            t.Sexo,
                            d.NombreDepartamento,
                            p.NombreProvincia,
                            di.NombreDistrito
                            FROM Trabajadores t
                            LEFT JOIN Departamento d ON t.IdDepartamento = d.Id
                            LEFT JOIN Provincia p ON t.IdProvincia = p.Id
                            LEFT JOIN Distrito di ON t.IdDistrito = di.Id";

                if (!string.IsNullOrEmpty(sexo))
                    sql += " WHERE Sexo = @Sexo";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(sexo))
                        cmd.Parameters.AddWithValue("@Sexo", sexo);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Trabajador
                            {
                                Id = reader.GetInt32(0),
                                TipoDocumento = reader.GetString(1),
                                NumeroDocumento = reader.GetString(2),
                                Nombres = reader.GetString(3),
                                Sexo = reader.GetString(4),
                                NombreDepartamento = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                NombreProvincia = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                NombreDistrito = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            });
                        }
                    }
                }
            }
            return View(lista);
        }
        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Crear(Trabajador trabajador)
        {
            if (!ModelState.IsValid)
                return View(trabajador);

            using (SqlConnection conn = Conexion())
            {
                conn.Open();
                string sql = @"
            INSERT INTO Trabajadores (TipoDocumento, NumeroDocumento, Nombres, Sexo, IdDepartamento, IdProvincia, IdDistrito)
            VALUES (@TipoDocumento, @NumeroDocumento, @Nombres, @Sexo, @IdDepartamento, @IdProvincia, @IdDistrito) ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TipoDocumento", trabajador.TipoDocumento ?? "");
                    cmd.Parameters.AddWithValue("@NumeroDocumento", trabajador.NumeroDocumento ?? "");
                    cmd.Parameters.AddWithValue("@Nombres", trabajador.Nombres ?? "");
                    cmd.Parameters.AddWithValue("@Sexo", trabajador.Sexo ?? "");
                    cmd.Parameters.AddWithValue("@IdDepartamento", (object?)trabajador.IdDepartamento ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdProvincia", (object?)trabajador.IdProvincia ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdDistrito", (object?)trabajador.IdDistrito ?? DBNull.Value);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            ModelState.AddModelError("NumeroDocumento", "El número de documento ya está registrado.");
                            return View(trabajador);
                        }
                        throw;
                    }
                }
            }
            return RedirectToAction("Index");
        }
        public JsonResult ObtenerDepartamentos()
        {
            var lista = new List<Departamento>();
            using (var conn = Conexion())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, NombreDepartamento FROM Departamento", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Departamento
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1)
                    });
                }
            }
            return Json(lista);
        }
        public JsonResult ObtenerProvincias(int idDepartamento)
        {
            var lista = new List<Provincia>();
            using (var conn = Conexion())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, NombreProvincia FROM Provincia WHERE IdDepartamento = @IdDepartamento", conn);
                cmd.Parameters.AddWithValue("@IdDepartamento", idDepartamento);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Provincia
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1)
                    });
                }
            }
            return Json(lista);
        }
        public JsonResult ObtenerDistritos(int idProvincia)
        {
            var lista = new List<Distrito>();
            using (var conn = Conexion())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, NombreDistrito FROM Distrito WHERE IdProvincia = @IdProvincia", conn);
                cmd.Parameters.AddWithValue("@IdProvincia", idProvincia);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Distrito
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1)
                    });
                }
            }
            return Json(lista);
        }
        public IActionResult Editar(int id)
        {
            Trabajador? trabajador = null;
            using (SqlConnection conn = Conexion())
            {
                conn.Open();

                string sql = "SELECT * FROM Trabajadores WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            trabajador = new Trabajador
                            {
                                Id = reader.GetInt32(0),
                                TipoDocumento = reader.GetString(1),
                                NumeroDocumento = reader.GetString(2),
                                Nombres = reader.GetString(3),
                                Sexo = reader.GetString(4),
                                IdDepartamento = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                IdProvincia = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                IdDistrito = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                            };
                        }
                    }
                }
            }
            if (trabajador == null)
                return NotFound();
            return View(trabajador);
        }
        [HttpPost]
        public IActionResult Editar(Trabajador trabajador)
        {
            if (!ModelState.IsValid)
                return View(trabajador);

            using (SqlConnection conn = Conexion())
            {
                conn.Open();

                string sql = @" UPDATE Trabajadores SET
                                TipoDocumento = @TipoDocumento,
                                NumeroDocumento = @NumeroDocumento,
                                Nombres = @Nombres,
                                Sexo = @Sexo,
                                IdDepartamento = @IdDepartamento,
                                IdProvincia = @IdProvincia,
                                IdDistrito = @IdDistrito
                                WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TipoDocumento", trabajador.TipoDocumento ?? "");
                    cmd.Parameters.AddWithValue("@NumeroDocumento", trabajador.NumeroDocumento ?? "");
                    cmd.Parameters.AddWithValue("@Nombres", trabajador.Nombres ?? "");
                    cmd.Parameters.AddWithValue("@Sexo", trabajador.Sexo ?? "");
                    cmd.Parameters.AddWithValue("@IdDepartamento", (object?)trabajador.IdDepartamento ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdProvincia", (object?)trabajador.IdProvincia ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdDistrito", (object?)trabajador.IdDistrito ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", trabajador.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            ModelState.AddModelError("NumeroDocumento", "El número de documento ya está registrado.");
                            return View(trabajador);
                        }
                        throw;
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            using (SqlConnection conn = Conexion())
            {
                conn.Open();
                string sql = "DELETE FROM Trabajadores WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }

    }
}
