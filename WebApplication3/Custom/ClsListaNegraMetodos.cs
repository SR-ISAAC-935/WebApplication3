using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplication3.Controllers;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;

namespace WebApplication3.Custom
{
    public class ClsListaNegraMetodos
    {
        private readonly LumitecContext _context;
        private readonly ILogger<ClsListaNegraMetodos> _logger;

        public ClsListaNegraMetodos(LumitecContext context, ILogger<ClsListaNegraMetodos> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InsertarListaNegraConDetalles(List<DetalleListaNegraDTO> detalles, decimal deudaTotal)
        {
            if (detalles == null || !detalles.Any())
            {
                throw new Exception("No se pueden insertar registros vacíos.");
            }
            foreach (var det in detalles)
            {
                Console.WriteLine(det.IdConsumidor+"\n"+det.idComprador);
            }

            var detallesTable = new DataTable();
            detallesTable.Columns.Add("IdProducto", typeof(int));
            detallesTable.Columns.Add("Cantidad", typeof(int));
            detallesTable.Columns.Add("Precio", typeof(decimal));

            foreach (var detalle in detalles)
            {
                detallesTable.Rows.Add(detalle.IdProducto, detalle.Cantidad, detalle.Precio);
            }

            using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("dbo.InsertarListaNegraConDetalles", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdConsumidor", detalles[0].IdConsumidor);
                    command.Parameters.AddWithValue("@IdComprador", detalles[0].idComprador);
                    command.Parameters.AddWithValue("@Deuda", deudaTotal);
                    command.Parameters.AddWithValue("@Detalles", detallesTable).SqlDbType = SqlDbType.Structured;
                    command.Parameters.AddWithValue("@IdEstado", 1);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task ModificarListaNegraConDetalles(List<DetalleListaNegraDTO> detalles, decimal deudaTotal)
        {
            if (detalles == null || !detalles.Any())
            {
                throw new Exception("No se pueden insertar registros vacíos.");
            }

            var detallesTable = new DataTable();
            detallesTable.Columns.Add("IdProducto", typeof(int));
            detallesTable.Columns.Add("Cantidad", typeof(int));
            detallesTable.Columns.Add("Precio", typeof(decimal));

            foreach (var detalle in detalles)
            {
                detallesTable.Rows.Add(detalle.IdProducto, detalle.Cantidad, detalle.Precio);
            }

            using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("[dbo].[AgregarProductosCredito]", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdConsumidor", detalles[0].IdConsumidor);
                    command.Parameters.AddWithValue("@Deuda", deudaTotal);
                    command.Parameters.AddWithValue("@Detalles", detallesTable).SqlDbType = SqlDbType.Structured;
                    command.Parameters.AddWithValue("@IdEstado", 1);

                    await command.ExecuteNonQueryAsync();
                }
            }

        }

        public List<DeudaDTO> ObtenerDeudasPorConsumidor(int idConsumidor)
        {
            return (from l in _context.ListaNegras
                    where l.IdConsumidor == idConsumidor
                    select new DeudaDTO
                    {
                        IdListado = l.IdListado,
                        Deuda = l.Deuda,
                        idEstado = l.IdEstado.ToString(),
                        FechaVenta = l.FechaVenta
                    }).ToList();
        }

        public List<DetalleDeudaDTO> ObtenerDetallesPorListado(int idListado)
        {
            return (from d in _context.DetalleListaNegra
                    join p in _context.Products
                        on d.IdProducto equals p.IdProduct
                    join e in _context.EstadosDeudores
                        on d.IdEstado equals e.IdEstado
                    where d.IdListado == idListado
                    select new DetalleDeudaDTO
                    {
                        Producto = p.ProductName,
                        Cantidad = d.Cantidad,
                        Precio = d.Precio,
                        Total = d.Precio * d.Cantidad,
                        status = e.Descripcion // Ejemplo
                    }).ToList();

        }


        public List<AbonoDTO> ObtenerAbono(int idListado)
        {
            var totalDeuda=_context.ListaNegras
                .Where(x=>x.IdListado == idListado).Select(x=>x.Deuda).FirstOrDefault();
            // Calcular la suma total de abonos para ese idListado
            var totalAbonos = _context.Abonos
                .Where(x => x.FacturaDeuda == idListado)
                .Sum(x => x.AbonoDeuda);

            // Obtener los detalles individuales de abonos
            var resultado = (from d in _context.ListaNegras
                             join a in _context.Abonos on d.IdListado equals a.FacturaDeuda
                             join c in _context.Consumidores on a.IdUsuario equals c.IdConsumidor
                             where a.FacturaDeuda == idListado
                             select new AbonoDTO
                             {
                                 id = d.IdListado,
                                 name = c.NombreConsumidor,
                                 Abonado = a.AbonoDeuda,
                                 FechaAbono = a.FechaAbono,
                                 total = totalAbonos+totalDeuda // se repite el total en cada fila (o podrías hacerlo solo en la primera)
                             }).ToList();

            return resultado;
        }
        public void ActualizarDeuda(int idListado, double abono)
        {
            if (idListado <= 0 || abono <= 0)
            {
                throw new ArgumentException("Parámetros inválidos.");
            }

            _context.Database.ExecuteSqlInterpolated($"EXEC [dbo].[ActualizarDeuda] @IdListado={idListado}, @Abono={abono}");
        }
        public string InsertarAbonos(int idListado, decimal abono, int idUsuario)
        {
            if (idListado > 0 && idUsuario > 0 && abono > 0)
            {
                string sql = @"INSERT INTO Abonos ([Factura Deuda], [ID Usuario], [Fecha Abono], [Abono deuda]) 
                       VALUES (@idListado, @idUsuario, @fecha, @abono)";

                var fecha = DateTime.Now;

                _context.Database.ExecuteSqlRaw(sql,
                    new[]
                    {
                new SqlParameter("@idListado", idListado),
                new SqlParameter("@idUsuario", idUsuario),
                new SqlParameter("@fecha", fecha),
                new SqlParameter("@abono", abono)
                    });

                return "Abono insertado correctamente.";
            }

            return $"Datos inválidos.{idListado} {idUsuario} {abono}";
        }



    }
}
